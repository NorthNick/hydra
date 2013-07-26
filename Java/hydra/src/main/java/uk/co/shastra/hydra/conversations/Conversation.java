package uk.co.shastra.hydra.conversations;

import rx.Observable;
import rx.subjects.PublishSubject;
import rx.subjects.Subject;
import uk.co.shastra.hydra.messaging.HydraMessage;
import uk.co.shastra.hydra.messaging.IHydraService;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.serializers.Serializer;
import uk.co.shastra.hydra.messaging.utils.EventHandlerNoData;
import uk.co.shastra.hydra.messaging.utils.EventNoData;

public class Conversation<TMessage> {

	private Serializer<TMessage> serializer;
	private IHydraService hydraService;
    private Subject<TMessage, TMessage> subject = PublishSubject.create();
    private boolean done = false;
    
	private String thisParty;
	private String remoteParty;
	private String topic;
	private String handle;
	private long lastSendSeq;
	private long lastRecvSeq;
	private boolean checkSeq;
	
	private EventNoData doneEvent = new EventNoData();
    public void addDoneHandler(EventHandlerNoData handler) { doneEvent.addHandler(handler); }
    public void removeDoneHandler(EventHandlerNoData handler) { doneEvent.removeHandler(handler); }

	public String getThisParty() { return thisParty; }
	public String getRemoteParty() { return remoteParty; }
	public String getTopic() { return topic; }
	public String getHandle() { return handle; }
	public long getLastSendSeq() { return lastSendSeq; }
	public long getLastRecvSeq() { return lastRecvSeq; }
	public boolean isCheckSeq() { return checkSeq; }

	void baseInit(IHydraService hydraService, String thisParty, String remoteParty, String topic, String handle, Serializer<TMessage> serializer)
    {
        this.hydraService = hydraService;
        this.thisParty = thisParty;
        this.remoteParty = remoteParty;
        this.topic = topic;
        this.handle = handle;
        this.serializer = serializer;
    }
    
    void onNext(long seq, TMessage message)
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
    
    public Observable<TMessage> getObservable() { return subject; }
    
    public void close()
    {
        done = true;
        doneEvent.raise(this);
    }
}
