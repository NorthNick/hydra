using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bollywell.Hydra.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ExtensionsTest
    {

        [TestMethod]
        public void TestMergeKeepDuplicates()
        {
            var arg = new List<List<int>> { new List<int> { 1, 3, 5 }, new List<int> { -1, 1, 2, 4 }, new List<int> { 6, 7 } };
            var expected = new List<int> { -1, 1, 1, 2, 3, 4, 5, 6, 7 };
            var res = arg.Merge(false).ToList();
            Assert.IsTrue(expected.SequenceEqual(res), "Merge of [{0}] with duplicates gives {1} instead of {2}", string.Join(", ", arg.Select(IntsToString)), IntsToString(res), IntsToString(expected));
        }

        [TestMethod]
        public void TestMergeDropDuplicates()
        {
            var arg = new List<List<int>> { new List<int> { 1, 3, 5 }, new List<int> { -1, 1, 2, 4 }, new List<int> { 6, 7 } };
            var expected = new List<int> { -1, 1, 2, 3, 4, 5, 6, 7 };
            var res = arg.Merge().ToList();
            Assert.IsTrue(expected.SequenceEqual(res), "Merge of [{0}] without duplicates gives {1} instead of {2}", string.Join(", ", arg.Select(IntsToString)), IntsToString(res), IntsToString(expected));
        }

        [TestMethod]
        public void TestMergeIsLazy()
        {
            // Elements go -4, -2, error, ...
            var badlist = Enumerable.Range(-2, 5).Select(i => i == 0 ? 1 / i : 2 * i);
            var arg = new List<IEnumerable<int>> { badlist, Enumerable.Range(-5, 5), Enumerable.Range(-3, 5) };
            var expected = new List<int> {-5, -4, -3, -2};
            var res = arg.Merge().Take(4);
            Assert.IsTrue(expected.SequenceEqual(res), "Partial enumeration of sequence merge should not raise an error");
        }

        private static string IntsToString(IEnumerable<int> ints)
        {
            return string.Format("[{0}]", string.Join(", ", ints.Select(i => i.ToString(CultureInfo.InvariantCulture))));
        }
    }
}
