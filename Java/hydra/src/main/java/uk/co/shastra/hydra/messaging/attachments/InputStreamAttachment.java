package uk.co.shastra.hydra.messaging.attachments;

import java.io.InputStream;

import org.apache.http.entity.ContentType;
import org.apache.http.entity.mime.content.ContentBody;
import org.apache.http.entity.mime.content.InputStreamBody;

public class InputStreamAttachment extends Attachment {

    private static final String DefaultContentType = "application/octet-stream";

    private InputStream data;

    public InputStream getData() { return data; }
    
    public InputStreamAttachment(String name, InputStream data) { this(name, data, DefaultContentType); }
    
	public InputStreamAttachment(String name, InputStream data, String contentType)
    {
    	super(name, contentType);
        this.data = data;
    }

	@Override
	public int dataLength() {
		// This is a dummy value since you can't get the length of an InputStream
		return 0;
	}

	@Override
	public ContentBody toContentBody() { return new InputStreamBody(data, ContentType.create(getContentType())); }
}
