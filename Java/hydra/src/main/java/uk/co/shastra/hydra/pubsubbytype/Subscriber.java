package uk.co.shastra.hydra.pubsubbytype;

import rx.Observable;
import rx.util.functions.Action1;
import rx.util.functions.Func1;
import uk.co.shastra.hydra.messaging.HydraMessage;
import uk.co.shastra.hydra.messaging.HydraService;
import uk.co.shastra.hydra.messaging.listeners.Listener;
import uk.co.shastra.hydra.messaging.messagefetchers.HydraByTopicByDestinationMessageFetcher;
import uk.co.shastra.hydra.messaging.messagefetchers.HydraByTopicMessageFetcher;
import uk.co.shastra.hydra.messaging.serializers.HydraJsonSerializer;
import uk.co.shastra.hydra.messaging.serializers.Serializer;
import uk.co.shastra.hydra.messaging.utils.Event;
import uk.co.shastra.hydra.messaging.utils.EventHandler;

public class Subscriber<TSub> {

    private Listener<HydraMessage> listener;
    private Observable<TSub> messageSource;
    private Serializer<TSub> serializer;
	
    private Event<TSub> messageInQueue = new Event<TSub>();

    public long getBufferDelayMs() { return listener.getBufferDelayMs(); }
    public void setBufferDelayMs(long bufferDelayMs) { listener.setBufferDelayMs(bufferDelayMs); }
    
    public long getPollIntervalMs() { return listener.getPollIntervalMs(); }
    public void setPollIntervalMs(long pollIntervalMs) { listener.setPollIntervalMs(pollIntervalMs); }
    
    public Subscriber(HydraService hydraService, Class<TSub> valueType) { this(hydraService, valueType, null, null, null); }
    /**
     * Subscribe to all published messages of type TSub
     * 
     * The serializer must match the one used by the message publisher
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param serializer The serializer to use for TSub objects. Defaults to HydraJsonSerializer
     */
    public Subscriber(HydraService hydraService, Class<TSub> valueType, Serializer<TSub> serializer) { this(hydraService, valueType, null, serializer, null); }

    public Subscriber(HydraService hydraService, Class<TSub> valueType, String thisParty) { this(hydraService, valueType, thisParty, null, null); }
    /**
     * Subscribe to all messages of type TSub sent to a specific destination
     * 
     * The serializer must match the one used by the message publisher. If thisParty is null, then the subscription will be to all published TSub messages
     * regardless of destination
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param valueType The type into which incoming messages are deserialised. Usually TSub.class where TSub is the concrete value of this class's generic parameter
     * @param thisParty The message destination (i.e. this app)
     * @param serializer The serializer to use for TSub objects. Defaults to HydraJsonSerializer
     * @param topic The topic to listen for. Defaults to the canonical name of valueType
     */
    public Subscriber(HydraService hydraService, Class<TSub> valueType, String thisParty, Serializer<TSub> serializer, String topic)
    {
        this.serializer = serializer == null ? new HydraJsonSerializer<TSub>(valueType) : serializer;
        String messageTopic = topic == null ? valueType.getCanonicalName() : topic;
        if (thisParty == null) {
            listener = hydraService.getListener(new HydraByTopicMessageFetcher(messageTopic));
        } else {
            listener = hydraService.getListener(new HydraByTopicByDestinationMessageFetcher(messageTopic, thisParty));
        }
        messageSource = listener.getObservable().map(new Func1<HydraMessage, TSub>() {
			@Override public TSub call(HydraMessage message) {
				return Subscriber.this.serializer.deserialize(message.getData());
			}
		});
        messageSource.subscribe(new Action1<TSub>() {
			@Override public void call(TSub message) { messageInQueue.raise(this, message);	}
		});
    }

    public void addMessageInQueueHandler(EventHandler<TSub> handler) { messageInQueue.addHandler(handler); }
    public void removeMessageInQueueHandler(EventHandler<TSub> handler) { messageInQueue.removeHandler(handler); }
    
    public Observable<TSub> getObservable() { return messageSource; }
    
    public void close()
    {
    	listener.close();
    }
}
