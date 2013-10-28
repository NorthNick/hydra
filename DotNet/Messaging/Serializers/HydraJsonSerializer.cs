using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Shastra.Hydra.Messaging.Serializers
{
    public class HydraJsonSerializer<TMessage> : ISerializer<TMessage>
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings {
            Converters = new List<JsonConverter> { new IsoDateTimeConverter() },
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        public string Serialize(TMessage obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, SerializerSettings);
        }

        public TMessage Deserialize(string str)
        {
            return JsonConvert.DeserializeObject<TMessage>(str, SerializerSettings);
        }
    }
}
