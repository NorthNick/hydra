package uk.co.shastra.hydra.messaging.messagefetchers;

import java.util.ArrayList;

import uk.co.shastra.hydra.messaging.TransportMessage;
import uk.co.shastra.hydra.messaging.messageids.MessageId;
import uk.co.shastra.hydra.messaging.storage.Store;
import uk.co.shastra.hydra.messaging.utils.ListUtils;

public class MultiFetcher<TMessage extends TransportMessage> implements MessageFetcher<TMessage> {

    private MessageFetcher<TMessage>[] fetchers;

	public MultiFetcher(MessageFetcher<TMessage>... fetchers)
    {
        this.fetchers = fetchers;
    }
    
	@Override
	public Iterable<TMessage> messagesAfterIdUpToSeq(Store store, MessageId startId, long lastSeq) {
		ArrayList<Iterable<TMessage>> messageLists = new ArrayList<Iterable<TMessage>>();
		for (MessageFetcher<TMessage> fetcher : fetchers) {
			messageLists.add(fetcher.messagesAfterIdUpToSeq(store, startId, lastSeq));
		}
		return ListUtils.Merge(messageLists);
	}

	@Override
	public Iterable<TMessage> messagesInSet(Store store, Iterable<MessageId> messageIds) {
		ArrayList<Iterable<TMessage>> messageLists = new ArrayList<Iterable<TMessage>>();
		for (MessageFetcher<TMessage> fetcher : fetchers) {
			messageLists.add(fetcher.messagesInSet(store, messageIds));
		}
		return ListUtils.Merge(messageLists);
	}

}
