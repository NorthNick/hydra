package uk.co.shastra.hydra.messaging.attachments;

import org.apache.http.entity.ContentType;
import org.apache.http.entity.mime.content.ByteArrayBody;
import org.apache.http.entity.mime.content.ContentBody;

public class ByteArrayAttachment extends Attachment {
	
    private static final String DefaultContentType = "application/octet-stream";

    private byte[] data;

    public byte[] getData() { return data; }

    public ByteArrayAttachment(String name, byte[] data) { this(name, data, DefaultContentType); }
    
	public ByteArrayAttachment(String name, byte[] data, String contentType)
    {
    	super(name, contentType);
        this.data = data;
    }

	@Override
	public int dataLength() { return data.length; }

	@Override
	public ContentBody toContentBody() { return new ByteArrayBody(data, ContentType.create(getContentType()), ""); }

}
