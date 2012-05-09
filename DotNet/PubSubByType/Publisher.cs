using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Serializers;

namespace Bollywell.Hydra.PubSubByType
{
    public class Publisher<TPub>
    {
        private readonly IHydraService _hydraService;
        private static readonly string _topic = typeof (TPub).FullName;
        private readonly ISerializer<TPub> _serializer;

        public Publisher(IHydraService hydraService, ISerializer<TPub> serializer = null)
        {
            _hydraService = hydraService;
            _serializer = serializer ?? new HydraDataContractSerializer<TPub>();
        }

        public void Send(TPub message)
        {
            var hydraMessage = new HydraMessage { Topic = _topic, Data = _serializer.Serialize(message) };
            _hydraService.Send(hydraMessage);
        }
    }
}
