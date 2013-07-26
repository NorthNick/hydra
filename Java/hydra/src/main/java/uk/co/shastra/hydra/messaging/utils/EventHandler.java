package uk.co.shastra.hydra.messaging.utils;

public interface EventHandler<TEventData> {
	public void handle(Object source, TEventData data);
}
