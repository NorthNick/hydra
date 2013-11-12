package uk.co.shastra.hydra.messaging.storage;

import java.io.InputStream;

import org.ektorp.ViewQuery;

import com.fasterxml.jackson.databind.JsonNode;

import uk.co.shastra.hydra.messaging.attachments.Attachment;
import uk.co.shastra.hydra.messaging.messageids.MessageId;

public interface Store {
    String getName();
    ChangesWrapper getChanges(MessageId startId, long sinceSeq);
    long getLastSeq();
    MessageId saveDoc(JsonNode json);
    MessageId saveDoc(JsonNode json, Iterable<Attachment> attachments);
    // Although ViewQuery can include the view name, they're separate for similarity to the .NET interface
    Iterable<JsonNode> getDocs(String viewName, ViewQuery options);
    InputStream getAttachment(Attachment attachment);
    ServerDistanceInfo measureDistance();
}
