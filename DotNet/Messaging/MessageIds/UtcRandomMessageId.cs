using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Bollywell.Hydra.Messaging.MessageIds
{
    public static partial class MessageIdManager
    {
        private class UtcRandomMessageId : IMessageId
        {
            // A message id is 32 lower-case hex characters. The first 14 are microseconds since 1 Jan 1970 UTC; the last 18 are random.
            private static readonly Regex IdPattern = new Regex("^[0-9a-f]{32}$", RegexOptions.Compiled);

            private readonly ulong _highBits, _lowBits;

            public UtcRandomMessageId(string couchId)
            {
                _highBits = ulong.Parse(couchId.Substring(0, 16), NumberStyles.AllowHexSpecifier);
                _lowBits = ulong.Parse(couchId.Substring(16, 16), NumberStyles.AllowHexSpecifier);
            }

            public UtcRandomMessageId(DateTime utcDate)
            {
                // DateTime only gives millisecond accuracy so, as a long, you get the stuff here.
                _highBits = (ulong) (utcDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) * 256000;
                _lowBits = 0;
            }

            public int CompareTo(IMessageId other)
            {
                var utcOther = (UtcRandomMessageId) other;
                var highCompare = _highBits.CompareTo(utcOther._highBits);
                return highCompare == 0 ? _lowBits.CompareTo(utcOther._lowBits) : highCompare;
            }

            public bool Equals(IMessageId other)
            {
                var utcOther = (UtcRandomMessageId) other;
                return _highBits == utcOther._highBits && _lowBits == utcOther._lowBits;
            }

            public static bool IsMessageId(string couchId)
            {
                return IdPattern.IsMatch(couchId);
            }

            public string ToDocId()
            {
                return _highBits.ToString("x16") + _lowBits.ToString("x16");
            }

            public DateTime ToDateTime()
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(_highBits / 256000);
            }

            public override string ToString()
            {
                return string.Format("{0:x16}{1:x16} Time {2}", _highBits, _lowBits, ToDateTime().ToString("o"));
            }
        }
    }
}