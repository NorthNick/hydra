using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Bollywell.Hydra.Messaging.MessageIds
{
    public static partial class MessageIdManager
    {
        private class UtcIdMessageId : IMessageId
        {
            // A message id is 14 lower-case hex characters followed by a suffix. The 14 hex characters are microseconds since 1 Jan 1970 UTC; the last 18 are random.
            private static readonly Regex IdPattern = new Regex("^[0-9a-f]{14}", RegexOptions.Compiled);

            //private readonly ulong _highBits, _lowBits;
            private readonly long _timeBits;
            private readonly string _suffix;

            public UtcIdMessageId(string couchId)
            {
                _timeBits = long.Parse(couchId.Substring(0, 14), NumberStyles.AllowHexSpecifier);
                _suffix = couchId.Substring(14);
            }

            public UtcIdMessageId(DateTime utcDate)
            {
                // DateTime only gives millisecond accuracy so, as a long, you get the stuff here.
                _timeBits = (long) (utcDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) * 1000;
                _suffix = "";
            }

            public int CompareTo(IMessageId other)
            {
                var utcOther = (UtcIdMessageId) other;
                var highCompare = _timeBits.CompareTo(utcOther._timeBits);
                return highCompare == 0 ? String.Compare(_suffix, utcOther._suffix, StringComparison.Ordinal) : highCompare;
            }

            public bool Equals(IMessageId other)
            {
                var utcOther = (UtcIdMessageId) other;
                return _timeBits == utcOther._timeBits && _suffix == utcOther._suffix;
            }

            public static bool IsMessageId(string couchId)
            {
                return couchId != null && IdPattern.IsMatch(couchId);
            }

            public string ToDocId()
            {
                return _timeBits.ToString("x14") + _suffix;
            }

            public DateTime ToDateTime()
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(_timeBits / 1000);
            }

            public override string ToString()
            {
                return string.Format("{0:x14}{1} Time {2}", _timeBits, _suffix, ToDateTime().ToString("o"));
            }
        }
    }
}