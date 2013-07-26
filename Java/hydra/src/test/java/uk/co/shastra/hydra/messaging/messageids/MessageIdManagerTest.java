package uk.co.shastra.hydra.messaging.messageids;

import static org.junit.Assert.*;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;

import org.junit.Test;

public class MessageIdManagerTest {
	
    @Test
    public void TestIdentifiesValidMessageIds()
    {
        Iterable<String> validIds = new ArrayList<String>(Arrays.asList(
            "abcdef12345678",           // no suffix
            "def123abcd1234990",        // numeric suffix
            "12345678900987helloworld"  // char suffix
        ));
        for (String validId : validIds) {
        	assertTrue(String.format("MessageIdManager.IsMessageId should succeed on %1$s", validId),  MessageIdManager.isMessageId(validId));
        }
    }

    @Test
    public void TestIdentifiesInvalidMessageIds()
    {
    	Iterable<String> invalidIds = new ArrayList<String>(Arrays.asList(
            "",                 // No characters
            "1234",             // Too short
            "Abcdef12345678",   // Upper case characters
            "abcdef1234567j",   // Non-hex characters
            null                // null
        ));
        for (String invalidId : invalidIds) {
            assertFalse(String.format("Message.IsMessageId should fail on %1$s", invalidId), MessageIdManager.isMessageId(invalidId));
        }
    }

    @Test
    public void TestPreservesMillisecondDateAccuracy()
    {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSZ");
        Date idDate = sdf.parse("2012-08-13T15:00:50.156-0000", new java.text.ParsePosition(0));
        Date anotherDate = sdf.parse("2012-08-13T15:00:50.155-0000", new java.text.ParsePosition(0));
        // Check that date parsing recognises milliseconds
        assertNotEquals(idDate, anotherDate);
        MessageId messageId = MessageIdManager.create(idDate);
        assertEquals(String.format("MessageId.ToDateTime for %1$s yields %2$s", idDate, messageId.toDateTime()),
        		idDate, messageId.toDateTime());
    }

    @Test
    public void TestPreservesDocId()
    {
    	final String docId = "56f3aa0b78095dabracadabra";
        MessageId messageId = MessageIdManager.create(docId);
        assertEquals(String.format("MessageId.ToDocId for %1$s yields %2$s", docId, messageId.toDocId()), docId, messageId.toDocId());
    }

}
