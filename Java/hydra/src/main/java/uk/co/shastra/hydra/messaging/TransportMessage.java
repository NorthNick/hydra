package uk.co.shastra.hydra.messaging;

import java.io.NotSerializableException;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonInclude.Include;
import com.fasterxml.jackson.core.JsonParser;
import com.fasterxml.jackson.core.TreeNode;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;

import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.messageids.MessageIdManager;

public class TransportMessage implements Comparable<TransportMessage> {

	// Ignore extra properties in JSON. Write dates in ISO format. Don't serialise null fields.
	private static ObjectMapper objectMapper = new ObjectMapper().
			configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false).
			configure(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS, false).
			setSerializationInclusion(Include.NON_NULL);
	
	private MessageId messageId;

	@JsonIgnore
	public MessageId getMessageId() { return messageId;	}
	public void setMessageId(MessageId messageId) {	this.messageId = messageId;	}
	
	public String getType() { return "message"; }
	public void setType(String type) {}
	
    protected void setFromCouchId(String couchId)
    {
        messageId = MessageIdManager.create(couchId);
    }
    
    public static <TMessage extends TransportMessage> TMessage hydrate(JsonNode json, Class<TMessage> valueType)
    {
    	JsonParser parser =	objectMapper.treeAsTokens((TreeNode) json);
    	TMessage res = null;
		try {
			// Use the valueType version of readValue on the assumption that TMessage will be non-generic. It's usually HydraMessage.   
			res = objectMapper.readValue(parser, valueType);
	        res.setFromCouchId(json.get("_id").textValue());
	    } catch (Exception e) {
			// TODO Handle error
		}
        return res;
    }
    
    public JsonNode toJson() throws NotSerializableException
    {
    	try {
    		return objectMapper.valueToTree(this);
    	} catch (Exception e) {
    		throw new NotSerializableException("TransportMessage: error serialising message. " + e.getMessage());
    	}
    }
    
	@Override
	public int compareTo(TransportMessage other) {
		return messageId.compareTo(other.getMessageId());
	}

}
