package uk.co.shastra.hydra.messaging.listeners;

import static org.junit.Assert.*;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Calendar;
import java.util.Date;
import java.util.concurrent.TimeUnit;

import org.junit.Before;
import org.junit.Test;

import rx.concurrency.TestScheduler;
import rx.util.functions.Action0;
import rx.util.functions.Action1;
import uk.co.shastra.hydra.messaging.HydraMessage;
import uk.co.shastra.hydra.messaging.HydraService;
import uk.co.shastra.hydra.messaging.messagefetchers.HydraByTopicMessageFetcher;
import uk.co.shastra.hydra.messaging.messagefetchers.MessageFetcher;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.messageids.MessageIdManager;
import uk.co.shastra.hydra.messaging.mocks.MockStore;
import uk.co.shastra.hydra.messaging.mocks.TestHydraMessage;
import uk.co.shastra.hydra.messaging.storage.NearestServerProvider;
import uk.co.shastra.hydra.messaging.storage.Provider;
import uk.co.shastra.hydra.messaging.storage.Store;

// See http://blogs.msdn.com/b/rxteam/archive/2012/06/14/testing-rx-queries-using-virtual-time-scheduling.aspx
public class ListenerTest {
	
	private TestScheduler scheduler;
	private Store store;
	private Provider provider;
	private HydraService service;
	private MessageFetcher<HydraMessage> fetcher;
	private Calendar startDate, sendDate;
	private Listener<HydraMessage> listener;
	private MessageId startId;
	private ArrayList<HydraMessage> messageList;

	@Before
	public void setUp() throws Exception {
		messageList = new ArrayList<HydraMessage>();
        // Any time after 1/1/1970 will do for startDate. CouchIds go wrong before that date as they are microseconds since 1/1/1970.
        startDate = Calendar.getInstance();
        startDate.set(2000,  1, 1, 0, 0, 0);
        scheduler = new TestScheduler();
        // We'd like to do this, but advanceTimeTo doesn't update now() properly
        //scheduler.advanceTimeTo(startDate.getTimeInMillis(), TimeUnit.MILLISECONDS);
        store = new MockStore("ListenerStore", "", scheduler);
        provider = new NearestServerProvider(new ArrayList<Store>(Arrays.asList(store)));
        service = new HydraService(provider);
        fetcher = new HydraByTopicMessageFetcher("Test");
        
        // startId is 1 minute before startDate
        Calendar startIdTime = (Calendar) startDate.clone();
        startIdTime.add(Calendar.MINUTE, -1);
        startId = MessageIdManager.create(startIdTime.getTime());
        // Dispose of the listener after 10 minutes. (res disposes of its subscription to poller, but poller itself also has to be disposed of otherwise the test never terminates.)
        // Note that the scheduler unsubscribes before the disposal, otherwise it will get an OnCompleted event from the listener, and res.messages.Count will be one larger.
        scheduler.schedule(new Action0() {
				@Override public void call() { listener.close(); }
			}, startDateOffset(10, TimeUnit.MINUTES), TimeUnit.MINUTES);
    }

	@Test
    public void testSingleMessage()
    {
        // Send a message after 20 seconds
		scheduler.schedule(new Action0(){
			@Override public void call() {
				try {
					service.send(makeMessage("Test", "Listener test", "TestSingleMessage"));
				} catch (Exception e) {}				
			}}, startDateOffset(20, TimeUnit.SECONDS), TimeUnit.SECONDS);
		
		// Start a Listener at 1 second and count the messages received
		scheduler.schedule(new Action0(){
			@Override public void call() {
				listener = new Listener<HydraMessage>(provider, fetcher, startId, null, scheduler);
				listener.getObservable().subscribe(new Action1<HydraMessage>() {
					@Override public void call(HydraMessage hydraMessage) { messageList.add(hydraMessage); }
				});
			}}, startDateOffset(1, TimeUnit.SECONDS), TimeUnit.SECONDS);

		// Execute 15 minutes worth of events (to get past the scheduler disposal event)
        scheduler.advanceTimeTo(startDateOffset(15, TimeUnit.MINUTES), TimeUnit.MINUTES);
        assertEquals("Should receive one message.", 1, messageList.size());
    }

	@Test
    public void testMultipleMessages()
    {
        // Send a few messages
		scheduler.schedule(new Action0(){
			@Override public void call() {
				try {
					service.send(makeMessage("Test", "Listener test", "TestMultipleMessages 1"));
				} catch (Exception e) {}				
			}}, startDateOffset(20, TimeUnit.SECONDS), TimeUnit.SECONDS);
		scheduler.schedule(new Action0(){
			@Override public void call() {
				try {
					service.send(makeMessage("Test", "Listener test", "TestMultipleMessages 2"));
				} catch (Exception e) {}				
			}}, startDateOffset(20, TimeUnit.SECONDS), TimeUnit.SECONDS);
		scheduler.schedule(new Action0(){
			@Override public void call() {
				try {
					service.send(makeMessage("Test", "Listener test", "TestMultipleMessages 3"));
				} catch (Exception e) {}				
			}}, startDateOffset(23, TimeUnit.SECONDS), TimeUnit.SECONDS);
		
		// Start a Listener at 1 second and count the messages received
		scheduler.schedule(new Action0(){
			@Override public void call() {
				listener = new Listener<HydraMessage>(provider, fetcher, startId, null, scheduler);
				listener.getObservable().subscribe(new Action1<HydraMessage>() {
					@Override public void call(HydraMessage hydraMessage) { messageList.add(hydraMessage); }
				});
			}}, startDateOffset(1, TimeUnit.SECONDS), TimeUnit.SECONDS);

		// Execute 15 minutes worth of events (to get past the scheduler disposal event)
        scheduler.advanceTimeTo(startDateOffset(15, TimeUnit.MINUTES), TimeUnit.MINUTES);
        assertEquals("Should receive three messages.", 3, messageList.size());
    }
	
	@Test
    public void testMissMessageSentBeforePolling()
    {
        // Send a message after 20 seconds
		scheduler.schedule(new Action0(){
			@Override public void call() {
				try {
					service.send(makeMessage("Test", "Listener test", "TestMissMessageSentBeforePolling"));
				} catch (Exception e) {}				
			}}, startDateOffset(20, TimeUnit.SECONDS), TimeUnit.SECONDS);

        // Start listening 10 seconds after the message was sent, so we should not receive it.
		scheduler.schedule(new Action0(){
			@Override public void call() {
				MessageId nowId = MessageIdManager.create(new Date(scheduler.now()));
				listener = new Listener<HydraMessage>(provider, fetcher, nowId, null, scheduler);
				listener.getObservable().subscribe(new Action1<HydraMessage>() {
					@Override public void call(HydraMessage hydraMessage) { messageList.add(hydraMessage); }
				});
			}}, startDateOffset(30, TimeUnit.SECONDS), TimeUnit.SECONDS);

		// Execute 15 minutes worth of events (to get past the scheduler disposal event)
        scheduler.advanceTimeTo(startDateOffset(15, TimeUnit.MINUTES), TimeUnit.MINUTES);
        assertEquals("Should receive no message, as none were sent after polling started.", 0, messageList.size());
    }
    
	@Test
    public void TestMessagesInBufferWindowAreOrdered()
    {
        // Ordinary message after 20 seconds
		scheduler.schedule(new Action0(){
			@Override public void call() {
				try {
					service.send(makeMessage("Test", "Listener test", "second"));
				} catch (Exception e) {}				
			}}, startDateOffset(20, TimeUnit.SECONDS), TimeUnit.SECONDS);
		
        // Send message after 22 seconds, predating the first by 1 second
		sendDate = (Calendar) startDate.clone();
		sendDate.add(Calendar.SECOND, 19);
		scheduler.schedule(new Action0(){
			@Override public void call() {
				try {
					service.send(makeTestMessage("Test", "Listener test", "first", sendDate.getTime()));
				} catch (Exception e) {}				
			}}, startDateOffset(22, TimeUnit.SECONDS), TimeUnit.SECONDS);

        // Set buffer window of 1500ms
		scheduler.schedule(new Action0(){
			@Override public void call() {
				listener = new Listener<HydraMessage>(provider, fetcher, startId, new ListenerOptions(1500, null), scheduler);
				listener.getObservable().subscribe(new Action1<HydraMessage>() {
					@Override public void call(HydraMessage hydraMessage) { messageList.add(hydraMessage); }
				});
			}}, startDateOffset(1, TimeUnit.SECONDS), TimeUnit.SECONDS);
		
		// Execute 15 minutes worth of events (to get past the scheduler disposal event)
        scheduler.advanceTimeTo(startDateOffset(15, TimeUnit.MINUTES), TimeUnit.MINUTES);
		assertEquals("Should receive two messages", 2, messageList.size());
		assertEquals("The second message sent should arrive before the first", "firstsecond", messageList.get(0).getData() + messageList.get(1).getData());
    }
	
	@Test
    public void testMessagesOutsideBufferWindowAreNotOrdered()
    {
        // Ordinary message after 20 seconds
		scheduler.schedule(new Action0(){
			@Override public void call() {
				try {
					service.send(makeMessage("Test", "Listener test", "second"));
				} catch (Exception e) {}				
			}}, startDateOffset(20, TimeUnit.SECONDS), TimeUnit.SECONDS);
		
        // Send message after 22 seconds, predating the first by 1 second
		sendDate = (Calendar) startDate.clone();
		sendDate.add(Calendar.SECOND, 19);
		scheduler.schedule(new Action0(){
			@Override public void call() {
				try {
					service.send(makeTestMessage("Test", "Listener test", "first", sendDate.getTime()));
				} catch (Exception e) {}				
			}}, startDateOffset(22, TimeUnit.SECONDS), TimeUnit.SECONDS);

        // Set buffer window of 500ms
		scheduler.schedule(new Action0(){
			@Override public void call() {
				listener = new Listener<HydraMessage>(provider, fetcher, startId, new ListenerOptions(500, null), scheduler);
				listener.getObservable().subscribe(new Action1<HydraMessage>() {
					@Override public void call(HydraMessage hydraMessage) { messageList.add(hydraMessage); }
				});
			}}, startDateOffset(1, TimeUnit.SECONDS), TimeUnit.SECONDS);
		
		// Execute 15 minutes worth of events (to get past the scheduler disposal event)
        scheduler.advanceTimeTo(startDateOffset(15, TimeUnit.MINUTES), TimeUnit.MINUTES);
		assertEquals("Should receive two messages", 2, messageList.size());
		assertEquals("The second message sent should arrive after the first", "secondfirst", messageList.get(0).getData() + messageList.get(1).getData());
    }
    
    
	private static HydraMessage makeMessage(String topic, String source, String data) 
	{
		HydraMessage message = new HydraMessage();
        message.setTopic(topic);
        message.setSource(source);
        message.setData(data);
		return message;
	}

	private static HydraMessage makeTestMessage(String topic, String source, String data, Date idDate) 
	{
		TestHydraMessage message = new TestHydraMessage();
        message.setTopic(topic);
        message.setSource(source);
        message.setData(data);
        message.setIdDate(idDate);
		return message;
	}
	
	/**
	 * Convert an offset from startDate to an absolute time.
	 * 
	 * @param offset Number of units to offset from startDate
	 * @param units measure unit of the offset
	 * @return The absolute number of units corresponding to the offset from startDate
	 */
	private long startDateOffset(int offset, TimeUnit units) {
		int divisor = 0;
		switch (units) {
		case SECONDS:
			divisor = 1000;
			break;
		case MINUTES:
			divisor = 60000;
			break;
		default:
			break;
		}
		return startDate.getTimeInMillis() / divisor + offset;
	}
}
