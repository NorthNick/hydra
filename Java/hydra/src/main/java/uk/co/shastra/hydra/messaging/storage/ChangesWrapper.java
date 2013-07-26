package uk.co.shastra.hydra.messaging.storage;

import uk.co.shastra.hydra.messaging.messageids.MessageId;

public class ChangesWrapper {
	public Iterable<MessageId> messageIds;
	public long lastSeq;
	
	public ChangesWrapper(Iterable<MessageId> messageIds, long lastSeq) {
		super();
		this.messageIds = messageIds;
		this.lastSeq = lastSeq;
	}
}
