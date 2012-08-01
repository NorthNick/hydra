using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Bollywell.Hydra.Messaging.MessageIds
{
    public static partial class MessageIdManager
    {
        private class LongMessageId : IMessageId
        {
            // MessageId is a 16-digit hex string being 14 hex digits for the number of microseconds since 1 Jan 1970, plus two digits for the entrypoint which default to 00.
            private static readonly Regex IdPattern = new Regex("^[0-9a-f]{16}$", RegexOptions.Compiled);

            private readonly long _messageId;

            public LongMessageId(string couchId)
            {
                // CouchIds are 16 hexadecimal digits
                _messageId = long.Parse(couchId, NumberStyles.AllowHexSpecifier);
            }

            public LongMessageId(DateTime utcDate)
            {
                // DateTime only gives millisecond accuracy so, as a long, you get the stuff here.
                _messageId = (long) (utcDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) * 256000;
            }

            public int CompareTo(IMessageId other)
            {
                return _messageId.CompareTo(((LongMessageId) other)._messageId);
            }

            public bool Equals(IMessageId other)
            {
                return _messageId == ((LongMessageId) other)._messageId;
            }

            public static bool IsMessageId(string couchId)
            {
                return IdPattern.IsMatch(couchId);
            }

            public string ToDocId()
            {
                return _messageId.ToString("x16");
            }

            public DateTime ToDateTime()
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(_messageId / 256000);
            }

            public override string ToString()
            {
                return string.Format("{0:x16} Entrypoint {1} Time {2}", _messageId, _messageId % 256, ToDateTime().ToString("o"));
            }
        }
    }
}