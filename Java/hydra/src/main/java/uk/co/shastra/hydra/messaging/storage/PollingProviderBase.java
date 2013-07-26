package uk.co.shastra.hydra.messaging.storage;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import rx.Observable;
import rx.util.functions.Action1;
import rx.util.functions.Func1;
import uk.co.shastra.hydra.messaging.utils.EventHandlerNoData;

public abstract class PollingProviderBase implements Provider {

    private static final int initialisationTimeoutMs = 5000;
    private String hydraServer;
    protected ServerDistance<ServerDistanceInfo> distances;
    protected HashMap<String, Store> storeDict = new HashMap<String, Store>();
    // Set after initialisation.
    protected boolean initialised = false;

	@Override public String getHydraServer() { return hydraServer; }
	public void setHydraServer(String hydraServer) {
		this.hydraServer = hydraServer;
		if (!initialised) finishedInitialisation();
	}
	
	protected boolean getInitialised() { return initialised; } 
	
	public long getDistanceIntervalMs() { return distances.getInterval(); }
	public void setDistanceIntervalMs(long interval) {distances.setInterval(interval); }
	
	protected PollingProviderBase(String hydraServer) { this(hydraServer, null, null); }
	protected PollingProviderBase(String hydraServer, String database) { this(hydraServer, database, null); }
    protected PollingProviderBase(String hydraServer, String database, Integer port) {
    	this(new ArrayList<String>(Arrays.asList(hydraServer)), database, port);
    }

    // NOTE: cannot declare PollingProviderBase(Iterable<String> hydraServers) as it has the same erased signature as
    // PollingProviderBase(Iterable<Store> stores).
    protected PollingProviderBase(Iterable<String> hydraServers, String database) { this(hydraServers, database, null); }
	protected PollingProviderBase(Iterable<String> hydraServers, String database, Integer port) {
		ArrayList<Store> stores = new ArrayList<Store>();
		for (String server : hydraServers) {
			stores.add(new CouchDbStore(server, database, port));
		}
	    init(stores);
	}

	protected PollingProviderBase(Iterable<Store> stores)
	{
		init(stores);
	}

	private void init(Iterable<Store> stores) {
		// TODO - sort out argument exception
	    //if (stores == null || !stores.Any()) throw new ArgumentException("At least one store must be supplied", "stores");
		for (Store store : stores) {
			storeDict.put(store.getName(), store);
		}
	    distances = new ServerDistance<ServerDistanceInfo>(storeDict.keySet(), getMeasureDistance(), getInitDistance());
	    distances.addFinishedInitialisationHandler(new EventHandlerNoData() {
			@Override public void handle(Object source) { finishedInitialisation(); }
		});
	    doSubscription(distances.getObservable());
	    distances.start();	
	}
	
	@Override
	public synchronized Store getStore(boolean waitForInitialisation) {
        if (waitForInitialisation && !initialised) {
            // Pause until initialisation completes, or the timeout fires.
        	waitForInitialisation();
        }
        return hydraServer == null ? null : storeDict.get(getHydraServer());
	}

	@Override
	public void serverError(String server) {
        // The lock ensures that multiple threads do not attempt to reset at the same time.
        // We could use double-checked locking, but even Jon Skeet is doubtful about it, and there would not be much performance gain.
        synchronized (this) {
            if (server.equals(getHydraServer())) {
                // The current server has gone offline. Inform Distances.
                distances.onDistanceInfo.call(new ServerDistanceInfo(server, false, Long.MAX_VALUE));
            }
		}
	}
	
	protected abstract void doSubscription(Observable<ServerDistanceInfo> observable);

	protected Action1<Iterable<String>> getInitDistance() {
		// Do nothing by default
		return new Action1<Iterable<String>>() {
			@Override public void call(Iterable<String> servers) {}
		};
	};
	
	protected Func1<String, ServerDistanceInfo> getMeasureDistance() {
		return new Func1<String, ServerDistanceInfo>() {
			@Override public ServerDistanceInfo call(String server) { return storeDict.get(server).measureDistance(); }
		};
	}

    protected synchronized void finishedInitialisation()
    {
        if (!initialised) {
            initialised = true;
            notifyAll();
        }
    }
    
    /**
     * Wait up to initialisationTimeoutMs for initialisation to finish.
     * 
     * If there is a timeout, set initialised to prevent future waits.
     */
    private synchronized void waitForInitialisation() {
    	long endTime = System.currentTimeMillis() + initialisationTimeoutMs;
    	long remainingTime = initialisationTimeoutMs;
    	while (!initialised && remainingTime > 0) {
    		try {
				wait(remainingTime);
			} catch (InterruptedException e) {
			}
    		remainingTime = endTime - System.currentTimeMillis();
    	}
    	initialised = true;
    }
}
