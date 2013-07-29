package uk.co.shastra.hydra.messaging.serializers;

public interface Serializer<TMessage> {
    String serialize(TMessage obj) throws Exception;
    TMessage deserialize(String str) throws Exception;
}
