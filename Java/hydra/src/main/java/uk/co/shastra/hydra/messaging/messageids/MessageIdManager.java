package uk.co.shastra.hydra.messaging.messageids;

import java.util.Date;

public class MessageIdManager {

    public static MessageId create(String couchId)
    {
        return new UtcIdMessageId(couchId);
    }

    public static MessageId create(Date utcDate)
    {
        return new UtcIdMessageId(utcDate);
    }

    public static boolean isMessageId(String couchId)
    {
        return UtcIdMessageId.isMessageId(couchId);
    }
    
}
