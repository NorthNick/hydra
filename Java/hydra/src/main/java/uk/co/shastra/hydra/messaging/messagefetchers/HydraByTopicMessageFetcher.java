package uk.co.shastra.hydra.messaging.messagefetchers;

import org.ektorp.ComplexKey;

import uk.co.shastra.hydra.messaging.HydraMessage;
import uk.co.shastra.hydra.messaging.messageids.MessageId;

public class HydraByTopicMessageFetcher extends MessageFetcherBase<HydraMessage> {

	private String topic;
	private ComplexKey endKey;
    
	public HydraByTopicMessageFetcher(String topic) {
		super(HydraMessage.class);
		this.topic = topic;
		// TODO - check that "{}" is a suitable max value
		endKey = ComplexKey.of(topic, ComplexKey.emptyObject());
	}

	@Override
	protected String getViewName() { return "broadcastMessages"; }

    // The broadcastMessages view is indexed on [topic, id]
	@Override
	protected ComplexKey getMessageKey(MessageId id) { return ComplexKey.of(topic, id.toDocId()); }

	@Override
	protected ComplexKey getEndKey() { return endKey; }

}
