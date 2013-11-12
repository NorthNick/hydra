package uk.co.shastra.hydra.messaging.messagefetchers;

import java.util.ArrayList;

import org.ektorp.ComplexKey;
import org.ektorp.ViewQuery;

import com.fasterxml.jackson.databind.JsonNode;

import uk.co.shastra.hydra.messaging.TransportMessage;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.storage.Store;

public abstract class MessageFetcherBase<TMessage extends TransportMessage> implements MessageFetcher<TMessage> {

    private Class<TMessage> messageType;

	protected abstract String getViewName();
    protected abstract ComplexKey getMessageKey(MessageId id);
    protected abstract ComplexKey getEndKey();
    
    // TODO Constructors for JavaType etc
    protected MessageFetcherBase(Class<TMessage> messageType) {
    	this.messageType = messageType;
    }
    
	@Override
	public Iterable<TMessage> messagesAfterIdUpToSeq(Store store, MessageId startId, long lastSeq) {
		ArrayList<TMessage> res = new ArrayList<TMessage>();
		for (JsonNode json : AllMessagesFrom(store, startId)) {
			if (json.get("value").longValue() > lastSeq) continue;
			TMessage message = TransportMessage.hydrate(json, messageType);
			if (message.getMessageId().compareTo(startId) > 0) res.add(message);
		}
		return res;
	}

	@Override
	public Iterable<TMessage> messagesInSet(Store store, Iterable<MessageId> messageIds) {
		ArrayList<ComplexKey> keys = new ArrayList<ComplexKey>();
		for (MessageId messageId : messageIds) {
			keys.add(getMessageKey(messageId));
		}
    	ViewQuery options = new ViewQuery().includeDocs(true).keys(keys);
    	
    	Iterable<JsonNode> docs = store.getDocs(getViewName(), options);
    	ArrayList<TMessage> res = new ArrayList<TMessage>();
    	for (JsonNode doc : docs) {
    		res.add(TransportMessage.hydrate(doc, messageType));
    	}
    	
        return res;
	}

    private Iterable<JsonNode> AllMessagesFrom(Store store, MessageId fromId)
    {
        // CouchDb startkey is inclusive, so this returns messages including fromId
    	ViewQuery options = new ViewQuery().includeDocs(true).startKey(getMessageKey(fromId)).endKey(getEndKey());
        return store.getDocs(getViewName(), options);
    }
}
