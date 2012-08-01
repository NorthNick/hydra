using System;
using System.Runtime.Serialization;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.MessageIds;
using LoveSeat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging
{
    [DataContract]
    public abstract class TransportMessage : IComparable<TransportMessage>
    {
        public IMessageId MessageId { get; set; }
        [DataMember] public string Type
        {
            get { return "message"; }
            set { }
        }

        protected void SetFromCouchId(string couchId)
        {
            MessageId = MessageIdManager.Create(couchId);
        }

        /// <summary>
        /// Send this message to the currently configured message centre
        /// </summary>
        internal void Send(IConfigProvider configProvider)
        {
            // TODO: use the "get database and server together" call suggested in Poller
            try {
                // The type parameter to Document<T> is irrelevant as it is only used for deserialisation, and here we are only serialising
                var doc = configProvider.GetDb().CreateDocument(new Document<TransportMessage>(this).ToString());
                // TODO: deal with the case where posting fails but raises a CouchDb {error:xxx, reason:xxx} object and not an exception.
            } catch (Exception ex) {
                configProvider.ServerError(configProvider.HydraServer);
                throw new Exception("TransportMessage.Send: error sending message. " + ex.Message, ex);
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

        public int CompareTo(TransportMessage other)
        {
            return MessageId.CompareTo(other.MessageId);
        }
    }

}
