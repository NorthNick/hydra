package uk.co.shastra.hydra.messaging;

import uk.co.shastra.hydra.messaging.attachments.Attachment;

public class AugmentedMessage<TMessage> {

    public TMessage message;
    public Iterable<Attachment> attachments;
    
	public TMessage getMessage() { return message; }
	public void setMessage(TMessage message) { this.message = message; }
	
	public Iterable<Attachment> getAttachments() { return attachments; }
	public void setAttachments(Iterable<Attachment> attachments) { this.attachments = attachments; }

	public AugmentedMessage(TMessage message, Iterable<Attachment> attachments) {
		this.message = message;
		this.attachments = attachments;
	}
	
}
