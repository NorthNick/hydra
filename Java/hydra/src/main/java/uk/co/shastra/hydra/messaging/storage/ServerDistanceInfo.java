package uk.co.shastra.hydra.messaging.storage;

public class ServerDistanceInfo {
	public String name;
	public boolean isReachable;
	public long distance;
	
	public ServerDistanceInfo(String name, boolean isReachable, long distance) {
		super();
		this.name = name;
		this.isReachable = isReachable;
		this.distance = distance;
	}
	
}
