package uk.co.shastra.hydra.messaging.messagefetchers;

import uk.co.shastra.hydra.messaging.TransportMessage;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.storage.Store;

public interface MessageFetcher<TMessage extends TransportMessage> {
    // Messages with MessageId > startId and with SeqId <= lastSeq
    Iterable<TMessage> messagesAfterIdUpToSeq(Store store, MessageId startId, long lastSeq);
    Iterable<TMessage> messagesInSet(Store store, Iterable<MessageId> messageIds);
}
