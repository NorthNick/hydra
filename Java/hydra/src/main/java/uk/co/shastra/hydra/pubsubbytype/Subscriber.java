package uk.co.shastra.hydra.pubsubbytype;

import com.fasterxml.jackson.core.type.TypeReference;

import rx.Notification;
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
    private Observable<Notification<TSub>> messageSource;
    private Serializer<TSub> serializer;
	
    private Event<Notification<TSub>> messageInQueue = new Event<Notification<TSub>>();

    /**
     * @return The time in milliseconds to delay delivery so that out-of-sequence messages can be delivered in the right order.
     */
    public long getBufferDelayMs() { return listener.getBufferDelayMs(); }
    /**
     * @param bufferDelayMs The time in milliseconds to delay delivery so that out-of-sequence messages can be delivered in the right order.
     */
    public void setBufferDelayMs(long bufferDelayMs) { listener.setBufferDelayMs(bufferDelayMs); }
    
    /**
     * @return Time between polls for messages, in milliseconds
     */
    public long getPollIntervalMs() { return listener.getPollIntervalMs(); }
    /**
     * @param pollIntervalMs Time between polls for messages, in milliseconds
     */
    public void setPollIntervalMs(long pollIntervalMs) { listener.setPollIntervalMs(pollIntervalMs); }
    
    /**
     * Subscribe to all published messages of type TSub
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param valueType The type into which incoming messages are deserialised. Usually TSub.class where TSub is the concrete value of this class's generic parameter
     */
    public Subscriber(HydraService hydraService, Class<TSub> valueType) { this(hydraService, valueType, null, null, null); }
    /**
     * Subscribe to all published messages of type TSub
     * 
     * The serializer must match the one used by the message publisher
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param valueType The type into which incoming messages are deserialised. Usually TSub.class where TSub is the concrete value of this class's generic parameter
     * @param serializer The serializer to use for TSub objects. Defaults to HydraJsonSerializer
     */
    public Subscriber(HydraService hydraService, Class<TSub> valueType, Serializer<TSub> serializer) { this(hydraService, valueType, null, serializer, null); }
    /**
     * Subscribe to all messages of type TSub sent to a specific destination
     * 
     * If thisParty is null, then the subscription will be to all published TSub messages regardless of destination
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param valueType The type into which incoming messages are deserialised. Usually TSub.class where TSub is the concrete value of this class's generic parameter
     * @param thisParty The message destination (i.e. this app)
     */
    public Subscriber(HydraService hydraService, Class<TSub> valueType, String thisParty) { this(hydraService, valueType, thisParty, null, null); }   
    /**
     * Subscribe to all messages of type TSub sent to a specific destination
     * 
     * The serializer must match the one used by the message publisher. If thisParty is null, then the subscription will be to all published TSub messages
     * regardless of destination
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param valueType The type into which incoming messages are deserialised. Usually TSub.class where TSub is the concrete value of this class's generic parameter.
     * @param thisParty The message destination (i.e. this app)
     * @param serializer The serializer to use for TSub objects. Defaults to HydraJsonSerializer
     * @param topic The topic to listen for. Defaults to the canonical name of valueType
     */
    public Subscriber(HydraService hydraService, Class<TSub> valueType, String thisParty, Serializer<TSub> serializer, String topic)
    {
        this.serializer = serializer == null ? new HydraJsonSerializer<TSub>(valueType) : serializer;
        String messageTopic = topic == null ? valueType.getCanonicalName() : topic;
        init(hydraService, thisParty, messageTopic);
    } 
    /**
     * Subscribe to all messages of type TSub
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param valueTypeRef The type into which incoming messages are deserialised. Usually new TypeReference<TSub>() {} and used when TSub is itself a
     * type with a generic parameter so that TSub.class cannot be used
     * @param topic The topic to listen for
     */
    public Subscriber(HydraService hydraService, TypeReference<?> valueTypeRef, String topic) { this(hydraService, valueTypeRef, topic, null); }
    /**
     * Subscribe to all messages of type TSub sent to a specific destination
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param valueTypeRef The type into which incoming messages are deserialised. Usually new TypeReference<TSub>() {} and used when TSub is itself a
     * type with a generic parameter so that TSub.class cannot be used
     * @param topic The topic to listen for
     * @param thisParty The message destination (i.e. this app)
     */
    public Subscriber(HydraService hydraService, TypeReference<?> valueTypeRef, String topic, String thisParty) { 
    	this(hydraService, new HydraJsonSerializer<TSub>(valueTypeRef), topic, thisParty);
    }
    /**
     * Subscribe to all messages of type TSub sent to a specific destination
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param serializer The serializer to use for TSub objects
     * @param topic The topic to listen for
     */
    public Subscriber(HydraService hydraService, Serializer<TSub> serializer, String topic) { this(hydraService, serializer, topic, null); }
    /**
     * Subscribe to all messages of type TSub sent to a specific destination
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param serializer The serializer to use for TSub objects
     * @param topic The topic to listen for
     * @param thisParty The message destination (i.e. this app)
     */ 
    public Subscriber(HydraService hydraService, Serializer<TSub> serializer, String topic, String thisParty) {
    	this.serializer = serializer;
    	init(hydraService, thisParty, topic);
    }
    
    private void init(HydraService hydraService, String thisParty, String topic) {
		if (thisParty == null) {
            listener = hydraService.getListener(new HydraByTopicMessageFetcher(topic));
        } else {
            listener = hydraService.getListener(new HydraByTopicByDestinationMessageFetcher(topic, thisParty));
        }
        messageSource = listener.getObservable().map(new Func1<HydraMessage, Notification<TSub>>() {
			@Override public Notification<TSub> call(HydraMessage message) {
				return MessageNotification(message.getData());
			}
		});
        messageSource.subscribe(new Action1<Notification<TSub>>() {
			@Override public void call(Notification<TSub> message) { messageInQueue.raise(this, message);	}
		});
	}

    private Notification<TSub> MessageNotification(String data)
    {
        try {
            return new Notification<TSub>(serializer.deserialize(data));
        } catch (Exception ex) {
            return new Notification<TSub>(ex);
        }
    }
    
    public void addMessageInQueueHandler(EventHandler<Notification<TSub>> handler) { messageInQueue.addHandler(handler); }
    public void removeMessageInQueueHandler(EventHandler<Notification<TSub>> handler) { messageInQueue.removeHandler(handler); }
    
    public Observable<Notification<TSub>> getObservable() { return messageSource; }
    
    public void close()
    {
    	listener.close();
    }
}
