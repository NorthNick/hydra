package uk.co.shastra.hydra.messaging.attachments;

import org.apache.http.entity.ContentType;
import org.apache.http.entity.mime.content.ContentBody;
import org.apache.http.entity.mime.content.StringBody;

public class StringAttachment extends Attachment {

    private static final String TextContentType = "text/plain";
    private String data;

	public String getData() { return data; }
	
	public StringAttachment(String name, String data) {
		super(name, TextContentType);
		this.data = data;
	}

	@Override
	public int dataLength() { return data.length(); }

	@Override
	public ContentBody toContentBody() { return new StringBody(data, ContentType.TEXT_PLAIN); }

}
