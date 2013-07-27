package uk.co.shastra.hydra.messaging;

import java.io.NotSerializableException;

import uk.co.shastra.hydra.messaging.listeners.Listener;
import uk.co.shastra.hydra.messaging.listeners.StdListener;
import uk.co.shastra.hydra.messaging.listeners.ListenerOptions;
import uk.co.shastra.hydra.messaging.messagefetchers.MessageFetcher;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.storage.Provider;
import uk.co.shastra.hydra.messaging.storage.Store;

public class StdHydraService implements HydraService {

    private Provider provider;

    private ListenerOptions defaultListenerOptions;
    /**
     * @return Default ListenerOptions applied to new Listeners if they are not overridden in getListener. 
     */
    public ListenerOptions getDefaultListenerOptions() { return defaultListenerOptions; }
	/**
	 * @param defaultListenerOptions Default ListenerOptions to be applied to new Listeners created by getListener. 
	 */
	public void setDefaultListenerOptions(ListenerOptions defaultListenerOptions) { this.defaultListenerOptions = defaultListenerOptions; }
	
	/**
	 * Create a new HydraService using the given Provider and no default ListenerOptions.
	 * 
	 * @param provider Storage provider to use for the service.
	 */
	public StdHydraService(Provider provider) { this(provider, null); }
    /**
     * Create a new HydraService using the given Provider and default ListenerOptions.
     * 
     * @param provider Storage provider to use for the service.
     * @param defaultListenerOptions Default ListenerOptions applied to new Listeners if they are not overridden in getListener.
     */
    public StdHydraService(Provider provider, ListenerOptions defaultListenerOptions)
    {
        this.provider = provider;
        this.defaultListenerOptions = defaultListenerOptions;
    }
    
	/**
	 * Start listening for messages using the supplied MessageFetcher. 
	 * 
	 * @param messageFetcher The MessageFetcher with which to poll for messages.
	 * @return A Listener that makes incoming messages available.
	 */
	@Override
	public <TMessage extends TransportMessage> Listener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher) { return getListener(messageFetcher, null, null); }
	/**
	 * Listen for messages since startId using the supplied MessageFetcher. 
	 * 
	 * @param messageFetcher The MessageFetcher with which to poll for messages.
	 * @param startId Fetch all messages on or after this MessageId
	 * @return A Listener that makes incoming messages available.
	 */
	@Override
	public <TMessage extends TransportMessage> Listener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher, MessageId startId) { return getListener(messageFetcher, startId, null); }
	/**
	 * Listen for messages since startId using the supplied MessageFetcher, and with the supplied ListenerOptions
	 * 
	 * @param messageFetcher The MessageFetcher with which to poll for messages.
	 * @param startId Fetch all messages on or after this MessageId
	 * @param listenerOptions Options to configure the Listener behaviour 
	 * @return A Listener that makes incoming messages available.
	 */
	@Override
	public <TMessage extends TransportMessage> Listener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher, MessageId startId, ListenerOptions listenerOptions) {
        return new StdListener<TMessage>(provider, messageFetcher, startId, listenerOptions == null ? defaultListenerOptions : listenerOptions);
	}

	/**
	 * Send a message.
	 * 
	 * @param message The message to send.
	 * @return The MessageId of the sent message.
	 * @throws Exception If there is an error sending the message.
	 */
	@Override
	public <TMessage extends TransportMessage> MessageId send(TMessage message) throws Exception {
        Store store = provider.getStore(true);
        while (store != null) {
            try {
                return store.saveDoc(message.toJson());
            } catch (NotSerializableException e) {
            	// Rethrow error without invoking server error
            	throw e;
            } catch (Exception e) {
            	// Swallow error and mark server as offline
                provider.serverError(store.getName());
            }
            store = provider.getStore(true);
        }
        throw new Exception("HydraService.Send: Error sending message - all servers offline.");
	}

    /**
     * @return The name of the Hydra server currently in use.
     */
	@Override
	public String getServerName() { return provider.getHydraServer(); }

}
