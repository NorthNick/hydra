using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Serializers;

namespace Bollywell.Hydra.PubSubByType
{
    public class Publisher<TPub>
    {
        private static readonly string _topic = typeof (TPub).FullName;
        private readonly ISerializer<TPub> _serializer;

        public Publisher(ISerializer<TPub> serializer = null)
        {
            _serializer = serializer ?? new HydraDataContractSerializer<TPub>();
        }

        public void Send(TPub message)
        {
                var hydraMessage = new HydraMessage { Topic = _topic, Data = _serializer.Serialize(message) };
                hydraMessage.Send();
        }
    }
}
