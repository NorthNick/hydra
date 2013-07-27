package uk.co.shastra.hydra.messaging.listeners;

import java.util.ArrayList;
import java.util.Date;
import rx.Observable;
import rx.Scheduler;
import rx.Subscription;
import rx.concurrency.NewThreadScheduler;
import rx.subjects.PublishSubject;
import rx.subjects.Subject;
import rx.util.functions.Action1;
import rx.util.functions.Func0;
import rx.util.functions.Func1;
import uk.co.shastra.hydra.messaging.TransportMessage;
import uk.co.shastra.hydra.messaging.messagefetchers.MessageFetcher;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.messageids.MessageIdManager;
import uk.co.shastra.hydra.messaging.storage.ChangesWrapper;
import uk.co.shastra.hydra.messaging.storage.Provider;
import uk.co.shastra.hydra.messaging.storage.Store;
import uk.co.shastra.hydra.messaging.utils.Event;
import uk.co.shastra.hydra.messaging.utils.EventHandler;
import uk.co.shastra.hydra.messaging.utils.ListUtils;
import uk.co.shastra.hydra.messaging.utils.ObservableGenerator;

public class StdListener<TMessage extends TransportMessage> implements Listener<TMessage> {

    private static final long DefaultPollIntervalMs = 1000;
    private static final long DefaultBufferDelayMs = 0;
    
	private Provider provider;
	private MessageFetcher<TMessage> messageFetcher;
	private boolean closed = false;
	private Store store;
    private ArrayList<TMessage> messageBuffer = new ArrayList<TMessage>();
    private long lastSeq;
    private MessageId startId;
	private Subscription subscription;
    private Subject<TMessage, TMessage> subject = PublishSubject.create();
    private Scheduler scheduler;
	
	private long bufferDelayMs;
	private long pollIntervalMs;
	private MessageId lastId;

	
    public long getBufferDelayMs() { return bufferDelayMs; }
	public void setBufferDelayMs(long bufferDelayMs) { this.bufferDelayMs = bufferDelayMs; }
	public long getPollIntervalMs() { return pollIntervalMs; }
	public void setPollIntervalMs(long pollIntervalMs) { this.pollIntervalMs = pollIntervalMs; }
	/**
	 * @return The last Id raised to clients. While processing a message, this will be the Id of that message.
	 */
	public MessageId getLastId() { return lastId; }
	
	
	public StdListener(Provider provider, MessageFetcher<TMessage> messageFetcher)  { this(provider, messageFetcher, null, null, null); }
	public StdListener(Provider provider, MessageFetcher<TMessage> messageFetcher, MessageId startId) { this(provider, messageFetcher, startId, null, null); }
    public StdListener(Provider provider, MessageFetcher<TMessage> messageFetcher, MessageId startId, ListenerOptions listenerOptions) { this(provider, messageFetcher, startId, listenerOptions, null); }
	/**
     * Construct a Listener and start it polling
     * 
     * The polling interval is taken from Service.getConfig().getPollIntervalMs() and is dynamic: changes take effect after the next poll.
     * 
     * @param provider The Provider to use
     * @param messageFetcher MessageFetcher with which to poll
     * @param startId Only fetch messages with higher id than startId. Defaults to the id corresponding to now.
     * @param listenerOptions Default values for Listener options.
     * @param scheduler Scheduler to use for polling. Defaults to NewThreadScheduler.getInstance()
     */
    public StdListener(Provider provider, MessageFetcher<TMessage> messageFetcher, MessageId startId, ListenerOptions listenerOptions, Scheduler scheduler)
    {
        this.provider = provider;
        this.messageFetcher = messageFetcher;
        this.scheduler = scheduler == null ? NewThreadScheduler.getInstance() : scheduler;
        bufferDelayMs = listenerOptions == null ? DefaultBufferDelayMs : listenerOptions.bufferDelayMs;
        pollIntervalMs = listenerOptions == null || (listenerOptions.pollIntervalMs == null) ? DefaultPollIntervalMs : listenerOptions.pollIntervalMs;
        lastId = startId == null ? MessageIdManager.create(new Date(this.scheduler.now())) : startId;
        subscription = 
        	new ObservableGenerator<Iterable<TMessage>>(pollIntervalMs, pollIntervalMs, new Func0<Iterable<TMessage>>() {
				@Override public Iterable<TMessage> call() {
					return OnElapsed();
			}}, this.scheduler).
			getObservable().
			mapMany(new Func1<Iterable<TMessage>, Observable<TMessage>>() {
				// Flatten the list
				@Override public Observable<TMessage> call(Iterable<TMessage> messages) {
					return Observable.from(messages);
			}}).
			subscribe(new Action1<TMessage>() {
				@Override public void call(TMessage message) { onMessageInQueue(message); }
			});
    }
	
	private final Iterable<TMessage> noMessages = new ArrayList<TMessage>();

    private Iterable<TMessage> OnElapsed()
    {
        if (closed) return noMessages;

        try {
            return Poll();
        } catch (Exception e) {
            // TODO: detect what sort of error this was
            provider.serverError(store.getName());
            return noMessages;
        }
    }
    
	private Iterable<TMessage> Poll() {
        Store pollStore = provider.getStore(false);
        if (pollStore == null) {
            // Stores are all offline, or initialisation is incomplete. Do nothing and wait until a pollStore is available.
            return noMessages;
        }
        if (store == null || !pollStore.getName().equals(store.getName())) {
            // The server has changed, so reinitialise. As store is initially null, this will be also be called on the very first poll.
            store = pollStore;
            startId = getLastId();
            // Populate messageBuffer from startId
            lastSeq = store.getLastSeq();
            // Fetch messages from startId, ignoring ones after lastSeq.
            messageBuffer = ListUtils.IterableToArrayList(messageFetcher.messagesAfterIdUpToSeq(store, startId, lastSeq));
        } else {
            // Get changes from lastSeq. Fetch messages in the change set and put in messageBuffer.
            ChangesWrapper changes = store.getChanges(startId, lastSeq);
            lastSeq = changes.lastSeq;
            Iterable<MessageId> messageIds = changes.messageIds;
            if (messageIds.iterator().hasNext()) {
            	ArrayList<Iterable<TMessage>> argh = new ArrayList<Iterable<TMessage>>();
            	argh.add(messageBuffer);
            	argh.add(messageFetcher.messagesInSet(store, messageIds));
            	messageBuffer = ListUtils.Merge(argh);
            }
        }

        MessageId delayedId = MessageIdManager.create(new Date(scheduler.now() - getBufferDelayMs()));
        // Place messageBuffer messages <= delayedId in newMessages and remove them from messageBuffer
        // This slightly clunky structure prevents concurrent ArrayList access errors.
        ArrayList<TMessage> newMessages = new ArrayList<TMessage>();
        while (messageBuffer.size() > 0) {
        	TMessage message = messageBuffer.get(0);
        	if (message.getMessageId().compareTo(delayedId) <= 0) {
        		newMessages.add(message);
        		messageBuffer.remove(0);
        	} else {
        		break;
        	}
        }

        return newMessages;
	}
    
    protected void onMessageInQueue(TMessage message) {
        try {
            lastId = message.getMessageId();
            subject.onNext(message);
            messageInQueue.raise(this, message);
        } catch (Exception e) {
            // Swallow errors so that an error processing one message does not stop others from being processed.
            // TODO: log error
        }
	}
    
	@Override public Observable<TMessage> getObservable() { return subject; }
	
	private Event<TMessage> messageInQueue = new Event<TMessage>();
	@Override public void addMessageinQueueHandler(EventHandler<TMessage> handler) { messageInQueue.addHandler(handler); }
	@Override public void removeMessageinQueueHandler(EventHandler<TMessage> handler) { messageInQueue.removeHandler(handler); }
	
	@Override
	public void close() {
        if (!closed) {
	        subscription.unsubscribe();
	        subject.onCompleted();
	        closed = true;
        }
	}

}
