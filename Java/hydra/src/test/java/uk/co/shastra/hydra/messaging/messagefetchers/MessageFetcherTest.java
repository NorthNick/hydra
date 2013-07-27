package uk.co.shastra.hydra.messaging.messagefetchers;

import static org.junit.Assert.*;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Calendar;
import java.util.Date;

import org.junit.BeforeClass;
import org.junit.Test;

import uk.co.shastra.hydra.messaging.HydraMessage;
import uk.co.shastra.hydra.messaging.StdHydraService;
import uk.co.shastra.hydra.messaging.HydraService;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.messageids.MessageIdManager;
import uk.co.shastra.hydra.messaging.mocks.MockStore;
import uk.co.shastra.hydra.messaging.storage.NearestServerProvider;
import uk.co.shastra.hydra.messaging.storage.Provider;
import uk.co.shastra.hydra.messaging.storage.Store;

public class MessageFetcherTest {

    private static final String MessageSource = "MessageFetcherTest";
    private static final String Alternating1 = "Alternating1";
    private static final String Alternating2 = "Alternating2";
    private static final int AlternatingMessageCount = 5;
    private static Store singleMessageStore;
    private static Store alternatingStore;
    
	@BeforeClass
	public static void setUpBeforeClass() throws Exception {
        // Set up stores
        singleMessageStore = new MockStore("SingleMessageStore", "");
        Provider provider = new NearestServerProvider(new ArrayList<Store>(Arrays.asList(singleMessageStore)));
        HydraService service = new StdHydraService(provider);
        HydraMessage message = makeMessage("Test", "Test data");
        service.send(message);

        alternatingStore = new MockStore("AlternatingStore", "");
        provider = new NearestServerProvider(new ArrayList<Store>(Arrays.asList(alternatingStore)));
        service = new StdHydraService(provider);
        for (int ii=0; ii < AlternatingMessageCount; ii++) {
            service.send(makeMessage(Alternating1, String.format("%1$ss message %2$s", Alternating1, ii)));
            service.send(makeMessage(Alternating2, String.format("%1$ss message %2$s", Alternating2, ii)));
        }
	}


	@Test
	public void testSingleMessageByTopicFetcher() {
        assertEquals("LastSeq should be zero after sending one message", singleMessageStore.getLastSeq(), 0);

        Calendar cal = Calendar.getInstance();
        cal.setTime(new Date());
        cal.add(Calendar.HOUR_OF_DAY, -1);
        
        MessageFetcher<HydraMessage> fetcher = new HydraByTopicMessageFetcher("Test");
        Iterable<HydraMessage> messages = fetcher.messagesAfterIdUpToSeq(singleMessageStore, MessageIdManager.create(cal.getTime()), 1);
        assertEquals("Should receive one message for topic Test", 1, ((ArrayList<HydraMessage>)messages).size());

        fetcher = new HydraByTopicMessageFetcher("Other");

        messages = fetcher.messagesAfterIdUpToSeq(singleMessageStore, MessageIdManager.create(cal.getTime()), 1);
        assertEquals("Should receive no messages for topic Other", 0, ((ArrayList<HydraMessage>)messages).size());
	}
	
	@Test
    public void TestAlternatingByTopicFetcher()
    {
        assertEquals("LastSeq should be nine after sending ten messages", alternatingStore.getLastSeq(), AlternatingMessageCount * 2 - 1);

        Calendar cal = Calendar.getInstance();
        cal.setTime(new Date());
        cal.add(Calendar.HOUR_OF_DAY, -1);
        
        MessageFetcher<HydraMessage> fetcher = new HydraByTopicMessageFetcher(Alternating1);
        // All Alternating1 messages
        ArrayList<HydraMessage> messages = (ArrayList<HydraMessage>) fetcher.messagesAfterIdUpToSeq(alternatingStore, MessageIdManager.create(cal.getTime()), AlternatingMessageCount * 2);
        assertEquals(String.format("Should receive %1$s messages for topic %2$s", AlternatingMessageCount, Alternating1), AlternatingMessageCount, messages.size());

        // Initial messages up to a SeqId
        messages = (ArrayList<HydraMessage>) fetcher.messagesAfterIdUpToSeq(alternatingStore, MessageIdManager.create(cal.getTime()), 4);
        assertEquals(String.format("Should receive three messages for topic %1$s up to SeqId 4", Alternating1), 3, messages.size());

        // All messages after the first
        MessageId firstMessageId = messages.get(0).getMessageId();
        messages = (ArrayList<HydraMessage>) fetcher.messagesAfterIdUpToSeq(alternatingStore, firstMessageId, AlternatingMessageCount * 2);
        assertEquals(String.format("Should receive %1$s messages for topic %2$s after first MessageId", AlternatingMessageCount - 1, Alternating1), AlternatingMessageCount - 1, messages.size());

        // Messages after the first, up to a SeqId
        messages = (ArrayList<HydraMessage>) fetcher.messagesAfterIdUpToSeq(alternatingStore, firstMessageId, 4);
        assertEquals(String.format("Should receive two messages for topic %1$s after first MessageId, up to SeqId 4", Alternating1), 2, messages.size());

        fetcher = new HydraByTopicMessageFetcher("Other");
        messages = (ArrayList<HydraMessage>) fetcher.messagesAfterIdUpToSeq(alternatingStore, MessageIdManager.create(cal.getTime()), 1);
        assertEquals("Should receive no messages for topic Other", 0, messages.size());
    }

	private static HydraMessage makeMessage(String topic, String data) 
	{
		HydraMessage message = new HydraMessage();
        message.setTopic(topic);
        message.setSource(MessageSource);
        message.setData(data);
		return message;
	}
}
