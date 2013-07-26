package uk.co.shastra.hydra.pubsubbytype;

import uk.co.shastra.hydra.messaging.HydraMessage;
import uk.co.shastra.hydra.messaging.IHydraService;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.serializers.HydraJsonSerializer;
import uk.co.shastra.hydra.messaging.serializers.Serializer;

/**
 * Class for publishing Hydra messages of a given type. To be used in conjunction with the Subscriber class.
 *
 * @param <TPub> The type of messages to publish
 */
public class Publisher<TPub> {
	
    private IHydraService hydraService;
    private Serializer<TPub> serializer;
	private String topic;

    private String thisParty;
    private String remoteParty;
    
    /**
     * @return An identifier for the sender of the messages
     */
    public String getThisParty() { return thisParty; }
	public void setThisParty(String thisParty) { this.thisParty = thisParty; }
	
	/**
	 * @return Send messages to a specific destination
	 */
	public String getRemoteParty() { return remoteParty; }
	public void setRemoteParty(String remoteParty) { this.remoteParty = remoteParty; }

    public Publisher(IHydraService hydraService) { this(hydraService, null, null); }
    /**
     * Create a new Publisher to send TPub messages
     * 
     * The serializer must match the one used by the message subscriber
     * 
     * @param hydraService The Hydra service to use for sending messages
     * @param serializer The serializer to use for TPub objects. Defaults to HydraJsonSerializer
     * @param topic The topic for outgoing messages. Defaults to the canonical name of TPub
     */
    public Publisher(IHydraService hydraService, Serializer<TPub> serializer, String topic)
    {
        this.hydraService = hydraService;
        this.serializer = serializer == null ? new HydraJsonSerializer<TPub>() : serializer;
        this.topic = topic;
    }

    public MessageId send(TPub message) throws Exception { return send(message, null); }
    /**
     * 
     * Send a message
     * 
     * @param message The message to send
     * @param remoteParty Optional RemoteParty override
     * @return The id of the message sent
     * @throws Exception 
     */
    public MessageId send(TPub message, String remoteParty) throws Exception
    {
    	if (topic == null) topic = message.getClass().getCanonicalName();
    	
    	HydraMessage hydraMessage = new HydraMessage();
    	hydraMessage.setSource(thisParty);
    	hydraMessage.setDestination(remoteParty == null ? getRemoteParty() : remoteParty);
    	hydraMessage.setTopic(topic);
    	hydraMessage.setData(serializer.serialize(message));
        return hydraService.send(hydraMessage);
    }
}
