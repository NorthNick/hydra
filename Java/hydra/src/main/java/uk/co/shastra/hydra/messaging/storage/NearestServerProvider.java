package uk.co.shastra.hydra.messaging.storage;

import rx.Observable;
import rx.util.functions.Action1;

public class NearestServerProvider extends PollingProviderBase {

	/**
	 * Measure distance to server on a separate thread
	 *
	 */
	private class InitServer extends Thread {
		private String server;

		public InitServer(String server) {
			this.server = server;
		}

		@Override
		public void run() {
			getMeasureDistance().call(server);
		}
		
	}
	
    public long tolerance = 50;

    public NearestServerProvider(String hydraServer) { super(hydraServer); }
    public NearestServerProvider(String hydraServer, String database) { super(hydraServer, database); }
    public NearestServerProvider(String hydraServer, String database, Integer port) { super(hydraServer, database, port); }

    public NearestServerProvider(Iterable<String> hydraServers, String database) { super(hydraServers, database); }
    public NearestServerProvider(Iterable<String> hydraServers, String database, Integer port) { super(hydraServers, database, port); }

    public NearestServerProvider(Iterable<Store> stores) { super(stores); }

    
	@Override
	protected void doSubscription(Observable<ServerDistanceInfo> observable) {
		observable.subscribe(new Action1<ServerDistanceInfo>() {
			@Override public void call(ServerDistanceInfo sdi) {
		        if (sdi.isReachable && (getHydraServer() == null || 
		        		(!getHydraServer().equals(sdi.name) && sdi.distance < distances.getServerInfo().get(getHydraServer()).distance - tolerance))) {
		            // There is a better server than the current one
		            setHydraServer(sdi.name);
		        } else if (!sdi.isReachable && getHydraServer().equals(sdi.name)) {
		            // The current server is no longer responding - replace with the nearest reachable one, or null if none are reachable
		        	ServerDistanceInfo nearest = null;
		        	for (ServerDistanceInfo candidate : distances.getServerInfo().values()) {
		        		if (candidate.isReachable && (nearest == null || candidate.distance < nearest.distance))
		        			nearest = candidate;
		        	}
		        	setHydraServer(nearest == null ? null : nearest.name);
		        } else if (!initialised && !sdi.isReachable && distances.getServerInfo().size() == storeDict.size()) {
		            // All servers are unreachable so initialisation is done
		            finishedInitialisation();
		        }
			}
		});
	}

	@Override
	protected Action1<Iterable<String>> getInitDistance() {
        // Do a one-off poll of everything and discard the results, but only wait 1.5 seconds in case of very slow responses. For some reason the first connection to
        // a CouchDb server on localhost takes one second for the TCP connect phase, so this gets over that initial slow poll.
		return new Action1<Iterable<String>>() {
			@Override
			public void call(Iterable<String> servers) {
				for (String server : servers) {
					Thread thread = new InitServer(server);
					thread.start();
					try {
						thread.join(1500);
					} catch (InterruptedException e) {}
				}
			}
		};
	}

}
