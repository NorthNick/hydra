package uk.co.shastra.hydra.messaging;

import java.io.NotSerializableException;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.Map.Entry;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonInclude.Include;
import com.fasterxml.jackson.core.JsonParser;
import com.fasterxml.jackson.core.TreeNode;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.fasterxml.jackson.databind.node.ObjectNode;

import uk.co.shastra.hydra.messaging.attachments.Attachment;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.messageids.MessageIdManager;

public class TransportMessage implements Comparable<TransportMessage> {

	// Ignore extra properties in JSON. Write dates in ISO format. Don't serialise null fields.
	private static ObjectMapper objectMapper = new ObjectMapper().
			configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false).
			configure(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS, false).
			setSerializationInclusion(Include.NON_NULL);
	
	private MessageId messageId;
	private Iterable<Attachment> attachments;

	@JsonIgnore
	public MessageId getMessageId() { return messageId;	}
	public void setMessageId(MessageId messageId) {	this.messageId = messageId;	}
	
	@JsonIgnore
	public Iterable<Attachment> getAttachments() { return attachments; }
	public void setAttachments(Iterable<Attachment> attachments) { this.attachments = attachments; }
	
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
	        res.setAttachments(json.path("_attachments"));
	    } catch (Exception e) {
			// TODO Handle error
		}
        return res;
    }
    
    private void setAttachments(JsonNode attachments) {
        // The _attachments JSON object is of the form {"attachment1name" : {"stub" : true, "content_type" : "text\/plain", "length" : 125}, "attachment2name" : {...} }
        if (attachments == null) return;

        ArrayList<Attachment> attList = new ArrayList<Attachment>();
        Iterator<Entry<String, JsonNode>> iterator = ((ObjectNode)attachments).fields();
        while (iterator.hasNext()) {
        	Entry<String, JsonNode> property = iterator.next();
        	attList.add(new Attachment(property.getKey(), ((ObjectNode)property.getValue()).get("content_type").textValue(), messageId));
        }
        this.attachments = attList;
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
