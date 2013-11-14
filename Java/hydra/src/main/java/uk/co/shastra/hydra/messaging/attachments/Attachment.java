package uk.co.shastra.hydra.messaging.attachments;

import org.apache.http.entity.mime.content.ContentBody;

import uk.co.shastra.hydra.messaging.messageids.MessageId;

public class Attachment {
    private String contentType;
    private MessageId messageId;
    private String name;

	public String getContentType() { return contentType; }
	public MessageId getMessageId() { return messageId; }
	public String getName() { return name; }

	/**
	 * 
	 * @param name Name of the attachment
	 * @param contentType HTTP Content-type
	 */
	public Attachment(String name, String contentType) {
		this(name, contentType, null);
	}
	
	/**
	 * 
	 * @param name Name of the attachment
	 * @param contentType HTTP Content-type
	 * @param messageId Message id of the message to which the attachment belongs
	 */
	public Attachment(String name, String contentType, MessageId messageId) {
		this.name = name;
		this.contentType = contentType;
		this.messageId = messageId;
	}

	public int dataLength() { return 0; }
	
	public ContentBody toContentBody() { return null; }
	
}
