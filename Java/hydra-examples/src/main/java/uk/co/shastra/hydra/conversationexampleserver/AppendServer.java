package uk.co.shastra.hydra.conversationexampleserver;

import rx.Subscription;
import rx.util.functions.Action1;
import uk.co.shastra.hydra.conversationexampledto.*;
import uk.co.shastra.hydra.conversations.Conversation;

public class AppendServer {
    private final Conversation<ConversationDto> conversation;
    private final Subscription subscription;
    private String suffix;

    public AppendServer(Conversation<ConversationDto> conversation)
    {
        this.conversation = conversation;
        subscription = conversation.getObservable().subscribe(new Action1<ConversationDto>() {
			@Override public void call(ConversationDto message) { onNext(message); }
		});
    }

    private void onNext(ConversationDto message)
    {
        // Ignore invalid messages
		try {
	        switch (message.getMessageType()) {
	                case INIT:
	                suffix = message.getData();
	                ConversationDto dtoInit = new ConversationDto();
	                dtoInit.setMessageType(MessageTypes.ACK);
					conversation.send(dtoInit);
	                break;
	            case REQUEST:
	            	ConversationDto dtoReq = new ConversationDto();
	                dtoReq.setMessageType(MessageTypes.RESPONSE);
	                dtoReq.setData(message.getData() + suffix);
	                conversation.send(dtoReq);
	                break;
	            case END:
	                subscription.unsubscribe();
	                conversation.close();
	                break;
	            default:
	            	break;
	        }
		} catch (Exception e) {}
    }
}
