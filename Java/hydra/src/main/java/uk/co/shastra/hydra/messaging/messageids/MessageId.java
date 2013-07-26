package uk.co.shastra.hydra.messaging.messageids;

import java.util.Date;

public interface MessageId extends Comparable<MessageId> {
    String toDocId();
    Date toDateTime();
}
