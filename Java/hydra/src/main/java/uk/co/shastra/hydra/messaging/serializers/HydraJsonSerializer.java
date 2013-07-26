package uk.co.shastra.hydra.messaging.serializers;

import com.fasterxml.jackson.annotation.JsonInclude.Include;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;

public class HydraJsonSerializer<TMessage> implements Serializer<TMessage> {

	// Ignore extra properties in JSON. Write dates in ISO format. Don't serialise null fields.
	private static ObjectMapper objectMapper = new ObjectMapper().
			configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false).
			configure(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS, false).
			setSerializationInclusion(Include.NON_NULL);
	
	private Class<TMessage> valueType;
	
	/**
	 * Constructor for serialisation only - this will fail if deserialize is called
	 */
	public HydraJsonSerializer() { this(null); }
	/**
	 * Constructor that can serialise and deserialise
	 * 
	 * @param valueType Type into which messages are deserialised usually TMessage.class where TMessage is the concrete type of this class's generic parameter
	 */
	public HydraJsonSerializer(Class<TMessage> valueType) {
		this.valueType = valueType;
	}

	@Override
	public String serialize(TMessage obj) {
		try {
			return objectMapper.writeValueAsString(obj);
		} catch (JsonProcessingException e) {
			// TODO handle error
		}
		return null;
	}

	@Override
	public TMessage deserialize(String str) {
    	TMessage res = null;
		try {
			// TODO - make this work with generics. See TransportMessage
			res = objectMapper.readValue(str, valueType);
	    } catch (Exception e) {
			// TODO Handle error
		}
        return res;
	}

}
