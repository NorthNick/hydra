package uk.co.shastra.hydra.messaging;

import java.io.InputStream;

import uk.co.shastra.hydra.messaging.attachments.Attachment;
import uk.co.shastra.hydra.messaging.listeners.Listener;
import uk.co.shastra.hydra.messaging.listeners.ListenerOptions;
import uk.co.shastra.hydra.messaging.messagefetchers.MessageFetcher;
import uk.co.shastra.hydra.messaging.messageids.MessageId;

public interface HydraService {
	/**
	 * Start listening for messages using the supplied MessageFetcher. 
	 * 
	 * @param messageFetcher The MessageFetcher with which to poll for messages.
	 * @return A Listener that makes incoming messages available.
	 */
	<TMessage extends TransportMessage> Listener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher);
	/**
	 * Listen for messages since startId using the supplied MessageFetcher. 
	 * 
	 * @param messageFetcher The MessageFetcher with which to poll for messages.
	 * @param startId Fetch all messages on or after this MessageId
	 * @return A Listener that makes incoming messages available.
	 */
	<TMessage extends TransportMessage> Listener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher, MessageId startId);
	/**
	 * Listen for messages since startId using the supplied MessageFetcher, and with the supplied ListenerOptions
	 * 
	 * @param messageFetcher The MessageFetcher with which to poll for messages.
	 * @param startId Fetch all messages on or after this MessageId
	 * @param listenerOptions Options to configure the Listener behaviour 
	 * @return A Listener that makes incoming messages available.
	 */
	<TMessage extends TransportMessage> Listener<TMessage> getListener(MessageFetcher<TMessage> messageFetcher, MessageId startId, ListenerOptions listenerOptions);

	/**
	 * Send a message.
	 * 
	 * @param message The message to send.
	 * @return The MessageId of the sent message.
	 * @throws Exception If there is an error sending the message.
	 */
	<TMessage extends TransportMessage> MessageId send(TMessage message) throws Exception;
	/**
	 * Send a message with attachments.
	 * 
	 * @param message The message to send.
	 * @param attachments Attachments to send with the message.
	 * @return The MessageId of the sent message.
	 * @throws Exception If there is an error sending the message.
	 */
	<TMessage extends TransportMessage> MessageId send(TMessage message, Iterable<Attachment> attachments) throws Exception;
	
	/**
	 * Get a message attachment.
	 * 
	 * @param attachment The Attachment to fetch.
	 * @return The attachment, as an InputStream.
	 * @throws Exception 
	 */
	InputStream getAttachment(Attachment attachment) throws Exception;
	
    /**
     * @return The name of the Hydra server currently in use.
     */
    String getServerName();
}
