package uk.co.shastra.hydra.messaging.utils;

import java.util.AbstractSet;
import java.util.HashSet;

public class Event<TEventData> {
	
	private AbstractSet<EventHandler<TEventData>> handlers = new HashSet<EventHandler<TEventData>>();
	
	public void addHandler(EventHandler<TEventData> handler) {
		handlers.add(handler);
	}
	
	public void removeHandler(EventHandler<TEventData> handler) {
		handlers.remove(handler);
	}
	
	public void removeAllHandlers() {
		handlers.clear();
	}
	
	public void raise(Object source, TEventData data) {
		for (EventHandler<TEventData> listener : handlers) {
			listener.handle(source, data);
		}
	}

}
