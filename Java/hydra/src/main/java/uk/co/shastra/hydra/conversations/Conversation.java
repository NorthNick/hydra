package uk.co.shastra.hydra.conversations;

import rx.Notification;
import rx.Observable;
import rx.subjects.PublishSubject;
import rx.subjects.Subject;
import uk.co.shastra.hydra.messaging.HydraMessage;
import uk.co.shastra.hydra.messaging.HydraService;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.serializers.Serializer;
import uk.co.shastra.hydra.messaging.utils.EventHandlerNoData;
import uk.co.shastra.hydra.messaging.utils.EventNoData;

/**
 * A Conversation is an exchange of messages between one sender and one recipient. Clients can engage in multiple simultaneous conversations
 * and messages on each are separate.
 *  
 * @author Nick North
 *
 * @param <TMessage> Type of messages comprising the Conversation.
 */
public class Conversation<TMessage> {

	private Serializer<TMessage> serializer;
	private HydraService hydraService;
    private Subject<Notification<TMessage>, Notification<TMessage>> subject = PublishSubject.create();
    private boolean done = false;
    
	private String thisParty;
	private String remoteParty;
	private String topic;
	private String handle;
	private long lastSendSeq;
	private long lastRecvSeq;
	private boolean checkSeq;
	
	private EventNoData doneEvent = new EventNoData();
    /**
     * Add a handler for the Done event, raised when a Conversation ends.
     * 
     * @param handler Object invoked when the Conversation ends.
     */
    public void addDoneHandler(EventHandlerNoData handler) { doneEvent.addHandler(handler); }
    /**
     * Remove a handler for the Done event.
     * 
     * @param handler Handler to be removed.
     */
    public void removeDoneHandler(EventHandlerNoData handler) { doneEvent.removeHandler(handler); }

	/**
	 * @return Identifier of this end of the Conversation.
	 */
	public String getThisParty() { return thisParty; }
	/**
	 * @return Identifier of the other end of the Conversation.
	 */
	public String getRemoteParty() { return remoteParty; }
	/**
	 * @return The Conversation topic. Defaults to the canonical name of the Conversation message type.
	 */
	public String getTopic() { return topic; }
	/**
	 * @return The Conversation handle. A GUID.
	 */
	public String getHandle() { return handle; }
	/**
	 * @return The sequence number of the last message sent. The first message has sequence number 1.
	 */
	public long getLastSendSeq() { return lastSendSeq; }
	/**
	 * @return The sequence number of the last message received.
	 */
	public long getLastRecvSeq() { return lastRecvSeq; }
	/**
	 * @return Whether out-of-sequence messages cause an error.
	 */
	public boolean isCheckSeq() { return checkSeq; }
	/**
	 * @param checkSeq Whether to check message sequence numbers.
	 */
	public void setCheckSeq(boolean checkSeq) { this.checkSeq = checkSeq; }
	
	void baseInit(HydraService hydraService, String thisParty, String remoteParty, String topic, String handle, Serializer<TMessage> serializer)
    {
        this.hydraService = hydraService;
        this.thisParty = thisParty;
        this.remoteParty = remoteParty;
        this.topic = topic;
        this.handle = handle;
        this.serializer = serializer;
    }
    
    void onNext(long seq, Notification<TMessage> message)
    {
        if (checkSeq && seq != lastRecvSeq + 1) {
            // Tell the client, but carry on and process the message.
        	subject.onError(new Exception(String.format("Sequence error. Expected %1$s, received %2$s", lastRecvSeq + 1, seq)));
        }
        lastRecvSeq = seq;
        subject.onNext(message);
    }
    
    /**
     * Send a message to the other end of the conversation
     * 
     * @param message The message to send
     * @return The id of the message sent
     * @throws Exception 
     */
    public MessageId send(TMessage message) throws Exception
    {
        if (done) return null;

        HydraMessage hydraMessage = new HydraMessage();
        hydraMessage.setSource(thisParty);
        hydraMessage.setDestination(remoteParty);
        hydraMessage.setTopic(topic);
        hydraMessage.setHandle(handle);
        hydraMessage.setSeq(lastSendSeq + 1);
        hydraMessage.setData(serializer.serialize(message));
        
        MessageId res = hydraService.send(hydraMessage);
        // Increment LastSendSeq after sending in case the Send fails.
        lastSendSeq++;
        return res;
    }
    
    /**
     * @return The Observable giving messages in this Conversation.
     */
    public Observable<Notification<TMessage>> getObservable() { return subject; }
    
    /**
     * Dispose of resources used by this Conversation.
     */
    public void close()
    {
        done = true;
        doneEvent.raise(this);
    }
}
