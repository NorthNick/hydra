package uk.co.shastra.hydra.messaging.listeners;

public class ListenerOptions {
    public long bufferDelayMs;
    public Long pollIntervalMs;
    
	public ListenerOptions(long bufferDelayMs, Long pollIntervalMs) {
		super();
		this.bufferDelayMs = bufferDelayMs;
		this.pollIntervalMs = pollIntervalMs;
	}
    
}
