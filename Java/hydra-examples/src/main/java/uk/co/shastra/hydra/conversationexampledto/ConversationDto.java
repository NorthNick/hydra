package uk.co.shastra.hydra.conversationexampledto;

public class ConversationDto {
    private MessageTypes messageType;
    private String data;
    
	public MessageTypes getMessageType() { return messageType; }
	public void setMessageType(MessageTypes messageType) { this.messageType = messageType; }
	public String getData() { return data; }
	public void setData(String data) { this.data = data; }
    
}
