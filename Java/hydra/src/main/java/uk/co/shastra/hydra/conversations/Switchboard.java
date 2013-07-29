package uk.co.shastra.hydra.conversations;

import java.util.HashMap;
import java.util.HashSet;
import java.util.UUID;

import com.fasterxml.jackson.core.type.TypeReference;

import rx.Notification;
import rx.Observable;
import rx.subjects.PublishSubject;
import rx.subjects.Subject;
import rx.util.functions.Action1;
import uk.co.shastra.hydra.messaging.HydraMessage;
import uk.co.shastra.hydra.messaging.HydraService;
import uk.co.shastra.hydra.messaging.listeners.Listener;
import uk.co.shastra.hydra.messaging.messagefetchers.HydraByTopicByDestinationMessageFetcher;
import uk.co.shastra.hydra.messaging.serializers.HydraJsonSerializer;
import uk.co.shastra.hydra.messaging.serializers.Serializer;
import uk.co.shastra.hydra.messaging.utils.EventHandlerNoData;

public class Switchboard<TMessage> {

    // Maps handles to their conversations
    private HashMap<String, Conversation<TMessage>> conversations = new HashMap<String, Conversation<TMessage>>();
    private HashSet<String> deadConversations = new HashSet<String>();
    private HashMap<String, EventHandlerNoData> handlers = new HashMap<String, EventHandlerNoData>();
    private Listener<HydraMessage> listener;
    private Subject<Conversation<TMessage>, Conversation<TMessage>> subject = PublishSubject.create();
    private Serializer<TMessage> serializer;
    private HydraService hydraService;
    private String thisParty;
    private String topic;
    
    /**
     * @return The time in milliseconds to delay delivery so that out-of-sequence messages can be delivered in the right order.
     */
    public long getBufferDelayMs() { return listener.getBufferDelayMs(); }
    /**
     * @param bufferDelayMs The time in milliseconds to delay delivery so that out-of-sequence messages can be delivered in the right order.
     */
    public void setBufferDelayMs(long bufferDelayMs) { listener.setBufferDelayMs(bufferDelayMs); }
    
    /**
     * Create a new Switchboard to listen for incoming conversations and initiate outgoing ones.
     * 
     * @param hydraService The HydraService with which this Switchboard communicates
     * @param valueType The type into which incoming messages are deserialised. Usually TMessage.class where TMessage is the concrete value of this class's generic parameter
     * @param thisParty Name of this end of the conversation. This will be the RemoteParty for anyone initiating a conversation with this app
     */
    public Switchboard(HydraService hydraService, Class<TMessage> valueType, String thisParty) { this(hydraService, valueType, thisParty, null, null); }
    /**
     * Create a new Switchboard to listen for incoming conversations and initiate outgoing ones.
     * 
     * @param hydraService The HydraService with which this Switchboard communicates
     * @param valueType The type into which incoming messages are deserialised. Usually TMessage.class where TMessage is the concrete value of this class's generic parameter
     * @param thisParty Name of this end of the conversation. This will be the RemoteParty for anyone initiating a conversation with this app
     * @param topic Topic of the conversation
     */
    public Switchboard(HydraService hydraService, Class<TMessage> valueType, String thisParty, String topic) { this(hydraService, valueType, thisParty, topic, null); }
    /**
     * Create a new Switchboard to listen for incoming conversations and initiate outgoing ones.
     * 
     * @param hydraService The HydraService with which this Switchboard communicates
	 * @param valueTypeRef The type into which incoming messages are deserialised. Usually new TypeReference<TSub>() {} and used when TSub is itself a
     * type with a generic parameter so that TSub.class cannot be used
     * @param thisParty Name of this end of the conversation. This will be the RemoteParty for anyone initiating a conversation with this app
     * @param topic Topic of the conversation
     */
    public Switchboard(HydraService hydraService, TypeReference<?> valueTypeRef, String thisParty, String topic) {
    	this(hydraService, null, thisParty, topic, new HydraJsonSerializer<TMessage>(valueTypeRef));
    }
    /**
     * Create a new Switchboard to listen for incoming conversations and initiate outgoing ones.
     * 
     * @param hydraService The HydraService with which this Switchboard communicates
     * @param valueType The type into which incoming messages are deserialised. Usually TMessage.class where TMessage is the concrete value of this class's generic parameter
     * @param thisParty Name of this end of the conversation. This will be the RemoteParty for anyone initiating a conversation with this app
     * @param topic Topic of the conversation
     * @param serializer Optional serialiser for messages. Defaults to HydraJsonSerializer
     */
    public Switchboard(HydraService hydraService, Class<TMessage> valueType, String thisParty, String topic, Serializer<TMessage> serializer)
    {
        this.hydraService = hydraService;
        this.thisParty = thisParty;
        this.topic = topic == null ? valueType.getCanonicalName() : topic;
        this.serializer = serializer == null ? new HydraJsonSerializer<TMessage>(valueType) : serializer;

        listener = hydraService.getListener(new HydraByTopicByDestinationMessageFetcher(this.topic, thisParty));
        listener.getObservable().subscribe(new Action1<HydraMessage>() {
			@Override public void call(HydraMessage message) { onMessage(message); }
		});
    }
    
    /**
     * Initiate a new conversation
     * 
     * @param remoteParty The other party in the conversation
     * @return The conversation
     */
    public Conversation<TMessage> newConversation(String remoteParty)
    {
    	// Creates a 32 hex digit uuid
        String handle = UUID.randomUUID().toString().replace("-", "");
        return createNewConversation(remoteParty, handle);
    }

    private void onMessage(HydraMessage message)
    {
        String handle = message.getHandle();
        if (deadConversations.contains(handle)) return;

        if (!conversations.containsKey(handle)) {
            createNewConversation(message.getSource(), handle);
        }
        conversations.get(handle).onNext(message.getSeq(), MessageNotification(message.getData()));
    }
    
    private Conversation<TMessage> createNewConversation(String remoteParty, String handle)
    {
    	Conversation<TMessage> conversation = new Conversation<TMessage>();
    	EventHandlerNoData handler = new EventHandlerNoData() {
			@Override public void handle(Object source) { conversationDoneEvent(source); }
		};
		handlers.put(handle, handler);
        conversation.addDoneHandler(handler);
        conversation.baseInit(hydraService, thisParty, remoteParty, topic, handle, serializer);
        conversations.put(handle, conversation);
        subject.onNext(conversation);
        return conversation;
    }

    @SuppressWarnings("unchecked")
	private void conversationDoneEvent(Object obj)
    {
    	Conversation<TMessage> conversation = (Conversation<TMessage>) obj;
    	EventHandlerNoData handler = handlers.remove(conversation.getHandle());
        conversation.removeDoneHandler(handler);
        String handle = conversation.getHandle();
        conversations.remove(handle);
        deadConversations.add(handle);
    }
    
    private Notification<TMessage> MessageNotification(String data)
    {
        try {
            return new Notification<TMessage>(serializer.deserialize(data));
        } catch (Exception ex) {
            return new Notification<TMessage>(ex);
        }
    }
    
    /**
     * @return The Observable giving new Conversations on this Switchboard.
     */
    public Observable<Conversation<TMessage>> getObservable() { return subject; }
    
    /**
     * Dispose of resources used by this Switchboard.
     */
    public void close()
    {
    	listener.close();
    }
}
