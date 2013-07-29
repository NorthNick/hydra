package uk.co.shastra.hydra.messaging.serializers;

import com.fasterxml.jackson.annotation.JsonInclude.Include;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;

public class HydraJsonSerializer<TMessage> implements Serializer<TMessage> {

	// Ignore extra properties in JSON. Write dates in ISO format. Allow types with no properties. Don't serialise null fields.
	private static ObjectMapper objectMapper = new ObjectMapper().
			configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false).
			configure(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS, false).
			configure(SerializationFeature.FAIL_ON_EMPTY_BEANS, false).
			setSerializationInclusion(Include.NON_NULL);
	
	// Exactly one of these should be non-null when deserialising - use that to determine which ObjectMapper.readValue to use. 
	private Class<TMessage> valueType;
	private TypeReference<?> valueTypeRef;
	
	/**
	 * Constructor for serialisation only - this will fail if deserialize is called
	 */
	public HydraJsonSerializer() { }
	/**
	 * Constructor that can serialise, and deserialise non-generic types.
	 * 
	 * @param valueType Type into which messages are deserialised usually TMessage.class where TMessage is the concrete type of this class's generic parameter
	 * TMessage must not be, or contain, generic types.
	 */
	public HydraJsonSerializer(Class<TMessage> valueType) {
		this.valueType = valueType;
	}
	/**
	 * Constructor that can serialise, and deserialise generic types.
	 * 
	 * @param valueType Type into which messages are deserialised usually new com.fasterxml.jackson.core.type.TypeReference<SomeGenericType>(){}
	 */
	public HydraJsonSerializer(TypeReference<?> valueTypeRef) {
		this.valueTypeRef = valueTypeRef;
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
	public TMessage deserialize(String str) throws Exception  {
    	TMessage res = null;
		if (valueType != null) {
			res = objectMapper.readValue(str, valueType);
		} else {
			res = objectMapper.readValue(str, valueTypeRef);
		}
        return res;
	}

}
