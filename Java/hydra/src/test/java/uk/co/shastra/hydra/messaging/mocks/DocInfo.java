package uk.co.shastra.hydra.messaging.mocks;

import java.util.Date;

public class DocInfo {
    public String destination;
    public String docId;
    public Date timestamp;
    public String topic;
    
	public DocInfo(String docId, String topic, String destination, Date timestamp) {
		super();
		this.destination = destination;
		this.docId = docId;
		this.timestamp = timestamp;
		this.topic = topic;
	}
    
}
