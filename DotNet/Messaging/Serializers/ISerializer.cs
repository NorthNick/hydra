namespace Shastra.Hydra.Messaging.Serializers
{
    public interface ISerializer<TMessage>
    {
        string Serialize(TMessage obj);
        TMessage Deserialize(string str);
    }
}