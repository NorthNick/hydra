package uk.co.shastra.hydra.messaging;

import uk.co.shastra.hydra.messaging.listeners.IListener;
import uk.co.shastra.hydra.messaging.listeners.ListenerOptions;
import uk.co.shastra.hydra.messaging.messagefetchers.MessageFetcher;
import uk.co.shastra.hydra.messaging.messageids.MessageId;

public interface IHydraService {
	<TMessage extends TransportMessage> IListener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher);
	<TMessage extends TransportMessage> IListener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher, MessageId startId);
	<TMessage extends TransportMessage> IListener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher, MessageId startId, ListenerOptions listenerOptions);
	<TMessage extends TransportMessage> MessageId send(TMessage message) throws Exception;
    String getServerName();
}
