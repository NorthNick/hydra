package uk.co.shastra.hydra.messaging.messagefetchers;

import org.ektorp.ComplexKey;

import uk.co.shastra.hydra.messaging.HydraMessage;
import uk.co.shastra.hydra.messaging.messageids.MessageId;

public class HydraByTopicByDestinationMessageFetcher extends MessageFetcherBase<HydraMessage> {

	private String topic;
	private String destination;
	private ComplexKey endKey;
    
	public HydraByTopicByDestinationMessageFetcher(String topic, String destination) {
		super(HydraMessage.class);
		this.topic = topic;
		this.destination = destination;
		// TODO - check that "{}" is a suitable max value
		endKey = ComplexKey.of(topic, destination, ComplexKey.emptyObject());
	}
	
	@Override
	protected String getViewName() { return "directedMessages"; }

    // The directedMessages view is indexed on [topic, destination, id]
	@Override
	protected ComplexKey getMessageKey(MessageId id) { return ComplexKey.of(topic, destination, id.toDocId()); }

	@Override
	protected ComplexKey getEndKey() { return endKey; }
	
}
