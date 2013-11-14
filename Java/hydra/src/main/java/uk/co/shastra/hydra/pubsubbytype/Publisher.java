package uk.co.shastra.hydra.pubsubbytype;

import uk.co.shastra.hydra.messaging.AugmentedMessage;
import uk.co.shastra.hydra.messaging.HydraMessage;
import uk.co.shastra.hydra.messaging.HydraService;
import uk.co.shastra.hydra.messaging.attachments.Attachment;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.serializers.HydraJsonSerializer;
import uk.co.shastra.hydra.messaging.serializers.Serializer;

/**
 * Class for publishing Hydra messages of a given type. To be used in conjunction with the Subscriber class.
 *
 * @param <TPub> The type of messages to publish
 */
public class Publisher<TPub> {
	
    private HydraService hydraService;
    private Serializer<TPub> serializer;
	private String topic;

    private String thisParty;
    private String remoteParty;
    
    /**
     * @return An identifier for the sender of the messages
     */
    public String getThisParty() { return thisParty; }
	/**
	 * @param thisParty An identifier for the sender of the messages
	 */
	public void setThisParty(String thisParty) { this.thisParty = thisParty; }
	
	/**
	 * @return Default destination for future messages. null means broadcast.
	 */
	public String getRemoteParty() { return remoteParty; }
	/**
	 * @param remoteParty The default destination for future messages. Set to null to broadcast.
	 */
	public void setRemoteParty(String remoteParty) { this.remoteParty = remoteParty; }

    /**
     * Create a new Publisher to send TPub messages
     * 
     * @param hydraService The Hydra service to use for sending messages
     */
    public Publisher(HydraService hydraService) { this(hydraService, null, null); }
    /**
     * Create a new Publisher to send TPub messages
     * 
     * This overload is useful when sending generic types, as they have no canonical name, so topic must be supplied.
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param topic The topic for outgoing messages. Defaults to the canonical name of TPub
     */
    public Publisher(HydraService hydraService, String topic) { this(hydraService, new HydraJsonSerializer<TPub>(), topic); }
    /**
     * Create a new Publisher to send TPub messages
     * 
     * The serializer must match the one used by the message subscriber
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param serializer The serializer to use for TPub objects. Defaults to HydraJsonSerializer
     * @param topic The topic for outgoing messages. Defaults to the canonical name of TPub
     */
    public Publisher(HydraService hydraService, Serializer<TPub> serializer, String topic)
    {
        this.hydraService = hydraService;
        this.serializer = serializer == null ? new HydraJsonSerializer<TPub>() : serializer;
        this.topic = topic;
    }

    /**
     * Broadcast a message (or send to getRemoteParty if that is non-null)
     * 
     * @param message The message to send
     * @return The id of the message sent
     * @throws Exception 
     */
    public MessageId send(TPub message) throws Exception { return send(message, null, null); }
    /**
     * Broadcast an augmented message (or send to getRemoteParty if that is non-null)
     * 
     * @param message The augmented message to send
     * @return The id of the message sent
     * @throws Exception 
     */
    public MessageId send(AugmentedMessage<TPub> message) throws Exception { return send(message.getMessage(), message.getAttachments(), null); }
    /**
     * Send a message to a specific remote party
     * 
     * @param message The message to send
     * @param remoteParty Optional RemoteParty override
     * @return The id of the message sent
     * @throws Exception 
     */
    public MessageId send(TPub message, String remoteParty) throws Exception { return send(message, null, remoteParty); }
    /**
     * Send an augmented message to a specific remote party
     * 
     * @param message The augmented message to send
     * @param remoteParty Optional RemoteParty override
     * @return The id of the message sent
     * @throws Exception 
     */
    public MessageId send(AugmentedMessage<TPub> message, String remoteParty) throws Exception { return send(message.getMessage(), message.getAttachments(), remoteParty); }
    /**
     * Broadcast a message with attachments (or send to getRemoteParty if that is non-null)
     * 
     * @param message The message to send
     * @param attachments Attachments to send with the message
     * @return The id of the message sent
     * @throws Exception 
     */
    public MessageId send(TPub message, Iterable<Attachment> attachments) throws Exception { return send(message, attachments, null); }
    /**
     * Send a message with attachments to a specific remote party
     * 
     * @param message The message to send
     * @param attachments Attachments to send with the message
     * @param remoteParty Optional RemoteParty override
     * @return The id of the message sent
     * @throws Exception 
     */
    public MessageId send(TPub message, Iterable<Attachment> attachments, String remoteParty) throws Exception
    {
    	if (topic == null) topic = message.getClass().getCanonicalName();
    	
    	HydraMessage hydraMessage = new HydraMessage();
    	hydraMessage.setSource(thisParty);
    	hydraMessage.setDestination(remoteParty == null ? getRemoteParty() : remoteParty);
    	hydraMessage.setTopic(topic);
    	hydraMessage.setData(serializer.serialize(message));
        return hydraService.send(hydraMessage, attachments);
    }
}
