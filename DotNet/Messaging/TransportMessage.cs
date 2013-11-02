using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Shastra.Hydra.Messaging.Attachments;
using Shastra.Hydra.Messaging.MessageIds;

namespace Shastra.Hydra.Messaging
{
    [DataContract]
    public abstract class TransportMessage : IComparable<TransportMessage>
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings {
            Converters = new List<JsonConverter> { new IsoDateTimeConverter() },
            ContractResolver = new CamelCasePropertyNamesContractResolver(), 
            NullValueHandling = NullValueHandling.Ignore,
        };
        private static readonly JsonSerializer Serializer = JsonSerializer.Create(SerializerSettings);

        public IMessageId MessageId { get; set; }

        public IEnumerable<Attachment> Attachments { get; set; }

        [DataMember] public string Type
        {
            get { return "message"; }
            set { }
        }

        /// <summary>
        /// Hydrate a CouchDb view row into a TMessage. The row has an id property, which is the document id, and a doc property which is the HydraMessage JSON.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static TMessage Hydrate<TMessage>(JToken row) where TMessage : TransportMessage
        {
            var res = row["doc"].ToObject<TMessage>();
            res.SetFromCouchId((string)row["id"]);
            res.SetAttachments(row["doc"]["_attachments"]);
            return res;
        }

        protected void SetFromCouchId(string couchId)
        {
            MessageId = MessageIdManager.Create(couchId);
        }

        private void SetAttachments(JToken attachments)
        {
            // The _attachments JSON object is of the form {"attachment1name" : {"stub" : true, "content_type" : "text\/plain", "length" : 125}, "attachment2name" : {...} }
            if (attachments == null) return;

            Attachments = attachments.Select(att => (JProperty) att).Select(prop => new Attachment(prop.Name, (string)prop.Value["content_type"], MessageId)).ToList();
        }

        /// <summary>
        /// Serialise the message for sending to a Store.
        /// </summary>
        /// <returns>The message serialised to a JSON object</returns>
        internal JObject ToJson()
        {
            try {
                return JObject.FromObject(this, Serializer);
            } catch (Exception ex) {
                throw new SerializationException("TransportMessage: Error serialising message. See inner exception for details.", ex);
            }
        }

        public int CompareTo(TransportMessage other)
        {
            return MessageId.CompareTo(other.MessageId);
        }
    }

}
