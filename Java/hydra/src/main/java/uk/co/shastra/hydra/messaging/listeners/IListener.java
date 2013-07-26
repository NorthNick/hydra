package uk.co.shastra.hydra.messaging.listeners;

import rx.Observable;
import uk.co.shastra.hydra.messaging.TransportMessage;
import uk.co.shastra.hydra.messaging.utils.EventHandler;

public interface IListener<TMessage extends TransportMessage> {
    long getBufferDelayMs();
    void setBufferDelayMs(long value);
    long getPollIntervalMs();
    void setPollIntervalMs(long value);
    Observable<TMessage> getObservable();
    void addMessageinQueueHandler(EventHandler<TMessage> handler);
    void removeMessageinQueueHandler(EventHandler<TMessage> handler);
    void close();
}
