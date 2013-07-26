package uk.co.shastra.hydra.messaging.serializers;

public interface Serializer<TMessage> {
    String serialize(TMessage obj);
    TMessage deserialize(String str);
}
