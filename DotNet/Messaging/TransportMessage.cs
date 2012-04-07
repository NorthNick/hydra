using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using LoveSeat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging
{
    [DataContract]
    public abstract class TransportMessage : IComparable<TransportMessage>
    {
        public IMessageId MessageId { get; set; }
        public IEntrypoint Entrypoint { get; set; }
        [DataMember] public string Type
        {
            get { return "message"; }
            set { }
        }

        protected void SetFromCouchId(string couchId)
        {
            MessageId = new LongMessageId(couchId);
            // Entrypoint is the last two digits of the 16-digit hexadecimal couchId
            Entrypoint = new StringEntrypoint(couchId.Substring(14));
        }

        /// <summary>
        /// Send this message to the currently configured message centre
        /// </summary>
        public void Send()
        {
            try {
                var config = Services.GetConfig();
                var db = new CouchClient(config.HydraServer, 5984, null, null, false, AuthenticationType.Basic).GetDatabase(config.Database);
                // The type parameter to Document<T> is irrelevant as it is only used for deserialisation, and here we are only serialising
                var doc = db.CreateDocument(new Document<TransportMessage>(this).ToString());
                // TODO: deal with the case where posting fails but raises a CouchDb {error:xxx, reason:xxx} object and not an exception.
            } catch (Exception ex) {
                throw new Exception("Cannot send message: " + ex.Message, ex);
            }

        }

        /// <summary>
        /// Hydrate a CouchDb view row into a TMessage. The row has an id property, which is the document id, and a doc property which is the HydraMessage JSON.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static TMessage Hydrate<TMessage>(JToken row) where TMessage : TransportMessage
        {
            var res = JsonConvert.DeserializeObject<TMessage>(row["doc"].ToString());
            res.SetFromCouchId((string)row["id"]);
            return res;
        }

        #region Message id and entrypoint methods

        public static IMessageId MessageIdForDate(DateTime utcDate)
        {
            // MessageId is a 16-digit hex string being 14 digits for the number of microseconds since 1 Jan 1970, plus two digits for the entrypoint which default to 00.
            // DateTime only gives millisecond accuracy so, as a long, you get the stuff here.
            return new LongMessageId((long)(utcDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds)*256000);
        }

        public static IMessageId MessageIdFromString(string messageIdStr)
        {
            return new LongMessageId(messageIdStr);
        }

        public static IEntrypoint EntryPointFromString(string entrypointStr)
        {
            return new StringEntrypoint(entrypointStr);
        }

        #endregion

        public int CompareTo(TransportMessage other)
        {
            return MessageId.CompareTo(other.MessageId);
        }
    }

    #region MessageIds

    public interface IMessageId : IComparable<IMessageId>, IEquatable<IMessageId>
    {
        string ToDocId();
        DateTime ToDateTime();
    }

    public class LongMessageId : IMessageId
    {
        // A message id is 16 lower-case hex characters
        private static readonly Regex IdPattern = new Regex("^[0-9a-f]{16}$", RegexOptions.Compiled);

        private readonly long _messageId;

        public LongMessageId(string couchId)
        {
            // CouchIds are 16 hexadecimal digits
            _messageId = long.Parse(couchId, NumberStyles.AllowHexSpecifier);
        }

        public LongMessageId(long messageId)
        {
            _messageId = messageId;
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

    #endregion

    #region Entrypoints

    public interface IEntrypoint : IEquatable<IEntrypoint>
    {
        object ToCouchKey();
    }

    internal class StringEntrypoint : IEntrypoint
    {
        private readonly string _entrypoint;

        internal StringEntrypoint(string entrypoint)
        {
            _entrypoint = entrypoint;
        }

        public override int GetHashCode()
        {
            return _entrypoint.GetHashCode();
        }

        public bool Equals(IEntrypoint other)
        {
            return _entrypoint.Equals(((StringEntrypoint) other)._entrypoint);
        }

        public object ToCouchKey()
        {
            return _entrypoint;
        }
    }

    #endregion

}
