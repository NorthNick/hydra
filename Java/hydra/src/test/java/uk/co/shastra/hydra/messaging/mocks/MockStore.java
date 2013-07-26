package uk.co.shastra.hydra.messaging.mocks;

import java.text.ParsePosition;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.HashSet;
import java.util.TreeSet;

import org.ektorp.ComplexKey;
import org.ektorp.ViewQuery;

import rx.Scheduler;
import rx.concurrency.CurrentThreadScheduler;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ArrayNode;
import com.fasterxml.jackson.databind.node.JsonNodeType;
import com.fasterxml.jackson.databind.node.ObjectNode;
import com.fasterxml.jackson.databind.node.POJONode;

import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.messageids.MessageIdManager;
import uk.co.shastra.hydra.messaging.storage.ChangesWrapper;
import uk.co.shastra.hydra.messaging.storage.ServerDistanceInfo;
import uk.co.shastra.hydra.messaging.storage.Store;

public class MockStore implements Store {

	private String name;
	private String suffix;
	private Scheduler scheduler;
    private ArrayList<DocInfo> docInfos = new ArrayList<DocInfo>();
    private HashMap<String, JsonNode> docs = new HashMap<String, JsonNode>();
	private long lastMicroseconds = 0;

	@Override public String getName() { return name; }

    public MockStore(String name) { this(name, null, null);}
    public MockStore(String name, String suffix) { this(name, suffix, null); }
    public MockStore(String name, String suffix, Scheduler scheduler)
    {
        this.name = name;
        this.suffix = suffix == null ? "" : suffix;
        this.scheduler = scheduler == null ? CurrentThreadScheduler.getInstance() : scheduler;
    }
    
	@Override
	public ChangesWrapper getChanges(MessageId startId, long sinceSeq) {
        // Get changes after sinceSeq, and drop messages at or before _startId
        Replicate();

        long lastSeq = getLastSeq();
        TreeSet<MessageId> idSet = new TreeSet<MessageId>();
        for (long ii = sinceSeq + 1; ii < lastSeq + 1; ii ++) {
        	MessageId messageId = MessageIdManager.create(docInfos.get((int) ii).docId);
        	if (messageId.compareTo(startId) > 0) idSet.add(messageId);
        }
		return new ChangesWrapper(idSet, lastSeq);
	}

	@Override
	public long getLastSeq() {
        // SeqId is the index of the last element of _docs
        return docs.size() - 1;
	}

	@Override
	public MessageId saveDoc(JsonNode json) {
        Replicate();

        Date idDate;
        String docId;
        // Allow the default DocId to be overridden in a TestHydraMessage
        if (json.get("idDate") != null) {
            idDate = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSZ").parse(json.get("idDate").textValue(), new ParsePosition(0));
            // This bypasses the uniqueness check, so don't specify the same date twice.
            docId = String.format("%1$014x%2$s", idDate.getTime() * 1000, suffix);
        } else {
            idDate = new Date(scheduler.now());
            docId = CreateDocId(idDate);
        }
        // The following is not good. It's based on the .NET MockStore, where matching the behaviour of LoveSeat means we want to store and return
        // a JSON object containing _id, value and doc, where doc is the document being saved. However Ektorp returns a document element containing
        // the _id and value fields as fields of the doc. So I've kept the .NET model, but bodged the extra fields into doc. This could be simplified.
        ((ObjectNode) json).put("_id", docId);
        ObjectNode stored = new ObjectMapper().createObjectNode();
        stored.put("doc", json);
        stored.put("_id", docId);
        String topic = json.has("topic") ? json.get("topic").asText() : null;
        String destination = json.has("destination") ? json.get("destination").asText() : null;
        DocInfo docInfo = new DocInfo(docId, topic, destination, idDate);
        // Lock out other SaveDoc calls so that we definitely get the right list length
        synchronized (this) {
            stored.put("value", docInfos.size());
            ((ObjectNode) json).put("value", docInfos.size());
            docInfos.add(docInfo);
        }
        docs.put(docId, stored);
        return MessageIdManager.create(docId);
	}

	@Override
	public Iterable<JsonNode> GetDocs(String viewName, ViewQuery options) {
        // We are only interested in viewNames "broadcastMessages" and "directedMessages"
        // options always has IncludeDocs=true and either:
        //   Keys = list of keys to get, each all [topic, couchId] or all [topic, destination, couchId] respectively. Or
        //   StartKey = [topic, couchId] or [topic, destination, couchId] and EndKey = [topic, maxvalue] or [topic, destination, maxvalue] respectively

        // Extract topic and possibly destination from here
        ArrayNode filterArray;
        String startId = null;
        HashSet<String> keySet = null;
        if (options.getStartKey() != null) {
            filterArray = (ArrayNode) ((ComplexKey) options.getStartKey()).toJson();
            startId = JsonNodeToString(filterArray.get(filterArray.size() - 1));
        } else {
        	ArrayNode keyArray = null;
			try {
				// getKeysAsJson gives a String like "{\"keys\" : [key0, key1, ...] }" where keyi is of the form "[val0, val1, ...]"
				keyArray = (ArrayNode) (new ObjectMapper().readTree(options.getKeysAsJson()).get("keys"));
			} catch (Exception e) {}
            // Empty array of keys for some reason
            if (keyArray.size() == 0) return new ArrayList<JsonNode>();
            filterArray = (ArrayNode) keyArray.get(0);
            keySet = new HashSet<String>(); //keyArray.Select(key => (string) key.Last));
            for (JsonNode key : keyArray) {
            	ArrayNode keyAsArray = (ArrayNode) key;
            	keySet.add(JsonNodeToString(keyAsArray.get(keyAsArray.size() - 1)));
            }
        }
        String topic = JsonNodeToString(filterArray.get(0));

        // Find the docs matching topic and possibly destination
        ArrayList<DocInfo> filteredDocInfos = new ArrayList<DocInfo>();
        if (viewName.equals("broadcastMessages")) {
        	for (DocInfo di : docInfos) {
				if (di.topic.equals(topic) && di.destination == null) filteredDocInfos.add(di);
			}
        } else if (viewName.equals("directedMessages")) {
            String destination = JsonNodeToString(filterArray.get(1));
        	for (DocInfo di : docInfos) {
				if (di.topic.equals(topic) && di.destination == destination) filteredDocInfos.add(di);
			}
        } else {
            // This view isn't implemented
        }

        // Return either those on or after startId, or in keySet
        TreeSet<String> filteredDocIds = new TreeSet<String>();
        if (startId != null) {
        	for (DocInfo di : filteredDocInfos) {
				if (di.docId.compareTo(startId) >= 0) filteredDocIds.add(di.docId); 
			}
        } else {
        	for (DocInfo di : filteredDocInfos) {
				if (keySet.contains(di.docId)) filteredDocIds.add(di.docId); 
			}
        }
        ArrayList<JsonNode> res = new ArrayList<JsonNode>();
        for (String docId : filteredDocIds) {
			res.add(docs.get(docId).get("doc"));
		}
        return res;
	}

	@Override
	public ServerDistanceInfo measureDistance() {
		return new ServerDistanceInfo(getName(), true, 10);
	}

    private void Replicate()
    {
        // For now do nothing
    }
    
    /**
     * Create a docid based on the date, incorporating the suffix, and ensuring monotonicity and uniqueness.
     * 
     * @param date The UTC date
     * @return A unique docid, higher in the sort order than any previous one
     */
    private String CreateDocId(Date date)
    {
        long microseconds = scheduler.now() * 1000;
        lastMicroseconds  = (microseconds > lastMicroseconds) ? microseconds : lastMicroseconds + 1;
        return String.format("%1$014x%2$s", lastMicroseconds, suffix);
    }
 
    private String JsonNodeToString(JsonNode json)
    {
    	if (json == null) return null;
    	
    	JsonNodeType jsonType = json.getNodeType();
    	if (jsonType == JsonNodeType.STRING) return json.asText();
    	return (String) ((POJONode) json).getPojo();
    }
}
