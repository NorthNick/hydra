package uk.co.shastra.hydra.messaging.storage;

import java.io.InputStream;
import java.net.MalformedURLException;
import java.util.ArrayList;
import java.util.TreeSet;

import org.apache.http.entity.ContentType;
import org.apache.http.entity.mime.MultipartEntityBuilder;
import org.apache.http.entity.mime.content.ContentBody;
import org.ektorp.CouchDbConnector;
import org.ektorp.CouchDbInstance;
import org.ektorp.StreamingChangesResult;
import org.ektorp.ViewQuery;
import org.ektorp.ViewResult;
import org.ektorp.changes.ChangesCommand;
import org.ektorp.changes.DocumentChange;
import org.ektorp.http.HttpClient;
import org.ektorp.http.HttpResponse;
import org.ektorp.http.HttpStatus;
import org.ektorp.http.StdHttpClient;
import org.ektorp.impl.StdCouchDbInstance;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.node.JsonNodeFactory;
import com.fasterxml.jackson.databind.node.ObjectNode;

import uk.co.shastra.hydra.messaging.attachments.Attachment;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.messageids.MessageIdManager;

public class CouchDbStore implements Store {

    private static final String DefaultDatabase = "hydra";
    private static final int DefaultPort = 5984;
    private static final String DesignDoc = "_design/hydra";
    
    private String database;
    private int port;
	private String name;
	private CouchDbConnector db;
	private String url;
    
	@Override
	public String getName() { return name; }
	
	public CouchDbStore(String server) { this("", server, null, null); }
	public CouchDbStore(String server, String database) { this("", server, database, null); }
	public CouchDbStore(String server, String database, Integer port) {
		this("", server, database, port);
		name = nameFromServerDetails(server, this.database, this.port);
	}
	
	public CouchDbStore(String name, String server, String database, Integer port) {
		this.name = name;
		this.database = database == null ? DefaultDatabase : database;
		this.port = port == null ? DefaultPort : port.intValue();
        // This URL checks both that the server is up, and that the view index is up to date
		this.url = String.format("http://%1$s:%2$s/%3$s/%4$s/_view/broadcastMessages?limit=0", server, this.port, this.database, DesignDoc);
		
		try {
			// TODO - may need to configure maxObjectSizeBytes better
			HttpClient httpClient = new StdHttpClient.Builder()
			        .url(String.format("http://%1$s:%2$s", server, this.port))
			        .maxObjectSizeBytes(100000)
			        .build();
			CouchDbInstance dbInstance = new StdCouchDbInstance(httpClient);
			db = dbInstance.createConnector(this.database, false);
		} catch (MalformedURLException e) {}
	}

	@Override
	public ChangesWrapper getChanges(MessageId startId, long sinceSeq) {
		// Use TreeSet so that MessageIds are sorted
		TreeSet<MessageId> res = new TreeSet<MessageId>();
		// Get changes as a stream, so that we get access to last_seq
		StreamingChangesResult changes = db.changesAsStream(new ChangesCommand.Builder().since(sinceSeq).build());
		for (DocumentChange change : changes) {
			String docId = change.getId();
			// Discard documents that are not messages e.g. design doc changes
			if (!MessageIdManager.isMessageId(docId)) continue;
			MessageId messageId = MessageIdManager.create(docId);
			// Discard MessageIds <= startId 
			if (messageId.compareTo(startId) <= 0) continue;
			res.add(messageId);
		}
		long lastSeq = changes.getLastSeq();
		changes.close();

		return new ChangesWrapper(res, lastSeq);
	}

	@Override
	public long getLastSeq() {
		return db.getDbInfo().getUpdateSeq();
	}

	@Override
	public MessageId saveDoc(JsonNode json) { return saveDoc(json, null); }
	
	@Override
	public MessageId saveDoc(JsonNode json, Iterable<Attachment> attachments) {
		if (attachments == null || !attachments.iterator().hasNext()) {
			// No attachments so just send as JSON
			db.create(json);
			return MessageIdManager.create(json.get("_id").textValue());
		} else {
			// Add _attachments to the JSON and send as multipart message
			// TODO: pass this to POST when it exists.
			InputStream stream = createMultipartContent(json, attachments);
			return null;
		}
	}

	@Override
	public Iterable<JsonNode> getDocs(String viewName, ViewQuery options) {
		ArrayList<JsonNode> res = new ArrayList<JsonNode>();
		for (ViewResult.Row row : db.queryView(options.designDocId(DesignDoc).viewName(viewName))) {
			// getDocAsNode gives back the CouchDb document as a JSON node i.e. a node with fields _id, _rev, topic, data etc 
			res.add(row.getDocAsNode());
		}
		return res;
	}
	
	@Override
	public InputStream getAttachment(Attachment attachment) {
		return db.getAttachment(attachment.getMessageId().toDocId(), attachment.getName());
	}
	
	@Override
	public ServerDistanceInfo measureDistance() {
        boolean responseOk = false;
        long elapsed = 0;
        try {
            long startTime = System.currentTimeMillis();
            HttpResponse response = db.getConnection().get(url);
            elapsed = System.currentTimeMillis() - startTime;
            responseOk = response.getCode() == HttpStatus.OK;
        }
        catch (Exception e) {
            // Swallow errors
        }
        return new ServerDistanceInfo(getName(), responseOk, responseOk ? elapsed : Long.MAX_VALUE);
	}

	private String nameFromServerDetails(String server, String database, int port) {
		return String.format("%1$s:%2$s:%3$s", server, port, database);
	}

	private static InputStream createMultipartContent(JsonNode json, Iterable<Attachment> attachments) {
        // The attachments are turned into an _attachments property on the JSON. The value is an object having one property
        // per attachment, whose name is the attachment name and whose value is as in JsonAttachment below. The document is sent as
        // a multipart/related MIME HTTP message, whose first part is the JSON, and whose subsequent parts are the attachments, in the
        // same order as the properties in the _attachments property.
		// Jackson ObjectNodes use a LinkedHashMap for storing properties, which means that they serialise in the order they're added, which
		// is the behaviour required by CouchDb.
		
		MultipartEntityBuilder mpBuilder = MultipartEntityBuilder.create();
		ArrayList<ContentBody> parts = new ArrayList<ContentBody>();
		JsonNodeFactory factory = JsonNodeFactory.instance;
		ObjectNode jsonParts = factory.objectNode();
		for (Attachment attachment : attachments) {
			parts.add(attachment.toContentBody());
			jsonParts.put(attachment.getName(), jsonAttachment(attachment.getContentType(), attachment.dataLength()));
		}
		((ObjectNode)json).put("_attachments", jsonParts);
		mpBuilder.addTextBody("", json.toString(), ContentType.APPLICATION_JSON);
		for (ContentBody part : parts) {
			mpBuilder.addPart("", part);
		}
		try {
			return mpBuilder.build().getContent();
		} catch (Exception e) {
			e.printStackTrace();
			return null;
		}
	}
	
    private static ObjectNode jsonAttachment(String contentType, long length)
    {
        ObjectNode res = JsonNodeFactory.instance.objectNode();
        res.put("follows", true);
        res.put("content_type", contentType);
        res.put("length", length);
        return res;
    }
}
