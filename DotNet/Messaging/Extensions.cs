// The code in this file is Copyright Nick North 2011, made freely available.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bollywell.Hydra.Messaging
{
    public static class Extensions
    {
        /// <summary>
        /// Lazily merge a sequence of sorted sequences into a single sorted sequence, using the default comparer.
        /// </summary>
        /// <typeparam name="T">Type of sequence elements. Must implement IComparable.</typeparam>
        /// <param name="sequences">Sequences to merge. The individual sequences will only be enumerated as far as necessary</param>
        /// <param name="dropDuplicates">Whether to drop duplicates. Defaults to true.</param>
        /// <returns>The merged sequence</returns>
        public static IEnumerable<T> Merge<T>(this IEnumerable<IEnumerable<T>> sequences, bool dropDuplicates = true) where T : IComparable<T>
        {
            return sequences.Merge(Comparer<T>.Default, dropDuplicates);
        }

        /// <summary>
        /// Lazily merge a sequence of sorted sequences into a single sorted sequence.
        /// </summary>
        /// <typeparam name="T">Type of sequence elements.</typeparam>
        /// <param name="sequences">Sequences to merge. The individual sequences will only be enumerated as far as necessary</param>
        /// <param name="comparer">The comparer for the sequence sort order</param>
        /// <param name="dropDuplicates">Whether to drop duplicates. Defaults to true.</param>
        /// <returns>The merged sequence</returns>
        public static IEnumerable<T> Merge<T>(this IEnumerable<IEnumerable<T>> sequences, IComparer<T> comparer, bool dropDuplicates = true)
        {
            // Put the sequences into an array of IEnumerable<T> containing the elements not yet returned.
            // Create a SortedSet containing the first element of each sequence and the index of the sequence it comes from.
            // On each iteration return the first element of the SortedSet and pull the next element, if any, from the corresponding sequence into the set.
            var remainder = sequences.ToArray();
            var buffer = new SortedSet<Tuple<T, int>>(new TupleComparer<T>(comparer));
            for (int i = 0; i < remainder.Count(); i++) {
                GetHead(remainder, i, buffer);
            }
            var val = buffer.Any() ? buffer.Min : null;
            while (buffer.Any()) {
                var res = val.Item1;
                yield return res;
                // Remove val (and any duplicates if required)
                do {
                    buffer.Remove(val);
                    GetHead(remainder, val.Item2, buffer);
                    val = buffer.Min;
                } while (dropDuplicates && buffer.Any() && comparer.Compare(val.Item1, res) == 0);
            }
        }

        /// <summary>
        /// Convert a long id to a string, left-padding with zeroes to ensure they compare correctly
        /// </summary>
        /// <param name="id">id to convert</param>
        /// <returns>The resulting string</returns>
        public static string ToDocId(this long id)
        {
            // long.MaxValue is 9,223,372,036,854,775,807 so pad to 19 digits
            return id.ToString().PadLeft(19, '0');
        }

        private static void GetHead<T>(IEnumerable<T>[] remainder, int index, SortedSet<Tuple<T, int>> buffer)
        {
            if (remainder[index].Any()) {
                buffer.Add(Tuple.Create(remainder[index].First(), index));
                remainder[index] = remainder[index].Skip(1);
            }
        }

        private class TupleComparer<T> : IComparer<Tuple<T, int>>
        {
            private readonly IComparer<T> _comparer;

            public TupleComparer(IComparer<T> comparer) { _comparer = comparer; }

            #region Implementation of IComparer<in Tuple<T,int>>

            public int Compare(Tuple<T, int> x, Tuple<T, int> y)
            {
                int res = _comparer.Compare(x.Item1, y.Item1);
                return res == 0 ? x.Item2.CompareTo(y.Item2) : res;
            }

            #endregion
        }
    }
}
