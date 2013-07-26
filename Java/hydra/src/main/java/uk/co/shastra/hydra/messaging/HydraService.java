package uk.co.shastra.hydra.messaging;

import uk.co.shastra.hydra.messaging.listeners.IListener;
import uk.co.shastra.hydra.messaging.listeners.Listener;
import uk.co.shastra.hydra.messaging.listeners.ListenerOptions;
import uk.co.shastra.hydra.messaging.messagefetchers.MessageFetcher;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.storage.Provider;
import uk.co.shastra.hydra.messaging.storage.Store;

public class HydraService implements IHydraService {

    private Provider provider;

    private ListenerOptions defaultListenerOptions;
    public ListenerOptions getDefaultListenerOptions() { return defaultListenerOptions; }
	public void setDefaultListenerOptions(ListenerOptions defaultListenerOptions) { this.defaultListenerOptions = defaultListenerOptions; }
	
	public HydraService(Provider provider) { this(provider, null); }
    public HydraService(Provider provider, ListenerOptions defaultListenerOptions)
    {
        this.provider = provider;
        this.defaultListenerOptions = defaultListenerOptions;
    }
    
	@Override
	public <TMessage extends TransportMessage> IListener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher) { return getListener(messageFetcher, null, null); }
	@Override
	public <TMessage extends TransportMessage> IListener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher, MessageId startId) { return getListener(messageFetcher, startId, null); }
	@Override
	public <TMessage extends TransportMessage> IListener<TMessage> getListener(
			MessageFetcher<TMessage> messageFetcher, MessageId startId, ListenerOptions listenerOptions) {
        return new Listener<TMessage>(provider, messageFetcher, startId, listenerOptions == null ? defaultListenerOptions : listenerOptions);
	}

	@Override
	public <TMessage extends TransportMessage> MessageId send(TMessage message) throws Exception {
        Store store = provider.getStore(true);
        while (store != null) {
            try {
                return store.saveDoc(message.toJson());
            } catch (Exception e) {
                provider.serverError(store.getName());
            }
            store = provider.getStore(true);
        }
        throw new Exception("HydraService.Send: Error sending message - all servers offline.");
	}

	@Override
	public String getServerName() { return provider.getHydraServer(); }

}
