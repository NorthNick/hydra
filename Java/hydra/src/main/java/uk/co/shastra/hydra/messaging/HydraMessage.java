package uk.co.shastra.hydra.messaging;

public class HydraMessage extends TransportMessage {
    private String source;
    private String destination;
    private String topic;
    private String subject;
    private String handle;
    private long seq;
    private String data;
    
	public String getSource() { return source; }
	public void setSource(String source) { this.source = source; }
	
	public String getDestination() { return destination; }
	public void setDestination(String destination) { this.destination = destination; }
	
	public String getTopic() { return topic; }
	public void setTopic(String topic) { this.topic = topic; }
	
	public String getSubject() { return subject; }
	public void setSubject(String subject) { this.subject = subject; }
	
	public String getHandle() { return handle; }
	public void setHandle(String handle) { this.handle = handle; }
	
	public long getSeq() { return seq; }
	public void setSeq(long seq) { this.seq = seq; }
	
	public String getData() { return data; }
	public void setData(String data) { this.data = data; }
}
