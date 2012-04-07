using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Bollywell.Hydra.Messaging.Serializers
{
    public class HydraDataContractSerializer<TMessage> : ISerializer<TMessage>
    {
        private static readonly DataContractSerializer _serializer = new DataContractSerializer(typeof(TMessage));

        public string Serialize(TMessage obj)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = new XmlTextWriter(stringWriter)) {
                _serializer.WriteObject(xmlWriter, obj);
                return stringWriter.ToString();
            }
        }

        public TMessage Deserialize(string str)
        {
            using (var stringReader = new StringReader(str))
            using (var xmlReader = XmlReader.Create(stringReader)) {
                return (TMessage) _serializer.ReadObject(xmlReader);
            }
        }
    }
}
