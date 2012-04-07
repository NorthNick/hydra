using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Bollywell.Hydra.Messaging.Serializers
{
    public class HydraJsonSerializer<TMessage> : ISerializer<TMessage>
    {
        private readonly JsonSerializerSettings _settings;

        public HydraJsonSerializer()
        {
            _settings = new JsonSerializerSettings();
            var converters = new List<JsonConverter> { new IsoDateTimeConverter() };
            _settings.Converters = converters;
            _settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            _settings.NullValueHandling = NullValueHandling.Ignore;
        }

        public string Serialize(TMessage obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, _settings);
        }

        public TMessage Deserialize(string str)
        {
            return JsonConvert.DeserializeObject<TMessage>(str, _settings);
        }
    }
}
