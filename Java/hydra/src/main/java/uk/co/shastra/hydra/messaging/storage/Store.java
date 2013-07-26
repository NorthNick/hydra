package uk.co.shastra.hydra.messaging.storage;

import org.ektorp.ViewQuery;

import com.fasterxml.jackson.databind.JsonNode;

import uk.co.shastra.hydra.messaging.messageids.MessageId;

public interface Store {
    String getName();
    ChangesWrapper getChanges(MessageId startId, long sinceSeq);
    long getLastSeq();
    MessageId saveDoc(JsonNode json);
    // Although ViewQuery can include the view name, they're separate for similarity to the .NET interface
    Iterable<JsonNode> GetDocs(String viewName, ViewQuery options);
    ServerDistanceInfo measureDistance();
}
