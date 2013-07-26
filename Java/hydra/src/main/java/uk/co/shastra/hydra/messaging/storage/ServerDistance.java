package uk.co.shastra.hydra.messaging.storage;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import rx.Observable;
import rx.Subscription;
import rx.concurrency.NewThreadScheduler;
import rx.subjects.PublishSubject;
import rx.subjects.Subject;
import rx.util.functions.Action0;
import rx.util.functions.Action1;
import rx.util.functions.Func0;
import rx.util.functions.Func1;
import uk.co.shastra.hydra.messaging.utils.*;

public class ServerDistance<TServerDistanceInfo extends ServerDistanceInfo> {
	
    // Default interval of 20 seconds.
    private static final long TimerInterval = 20000;

    private class DistanceMeasurer implements Func0<TServerDistanceInfo> {
    	private String server;

		public DistanceMeasurer(String server) {
			this.server = server;
		}

		@Override
		public TServerDistanceInfo call() {
			return measureDistance.call(server);
		}
    	
    }
    
    private List<String> servers;
    private Subject<TServerDistanceInfo, TServerDistanceInfo> subject = PublishSubject.create();
    private Subscription poller;
    private boolean initialisedRaised;
    private Action1<Iterable<String>> initAction;
    private Func1<String, TServerDistanceInfo> measureDistance;

    private long interval;
    /**
     * @return Gets or sets the the interval in milliseconds at which servers are pinged. Default 5 mins.
     */
    public long getInterval() { return interval; }
 	/**
 	 * @param interval The interval in milliseconds at which servers are pinged
 	 */
 	public void setInterval(long interval) { this.interval = interval; }
 	
    private HashMap<String, TServerDistanceInfo> serverInfo;
	/**
	 * @return TServerDistanceInfo item for each server being monitored.
	 */
	public HashMap<String, TServerDistanceInfo> getServerInfo() { return serverInfo; }

	/**
	 * @return Observable publishing server distances as they are measured
	 */
	public Observable<TServerDistanceInfo> getObservable() { return subject; }
	
	/**
 	 * Class to maintain ping time information to a collection of servers
 	 * 
 	 * @param servers Names or string representation of IP addresses of the servers to monitor
 	 * @param measureDistance Optional function to measure distance to a server
 	 * @param initAction Optional initialisation function, to be run asynchronously before polling starts.
 	 */
 	public ServerDistance(Iterable<String> servers, Func1<String, TServerDistanceInfo> measureDistance, Action1<Iterable<String>> initAction)
    {
        interval = TimerInterval;
        this.servers = new ArrayList<String>();
        for (String server : servers) this.servers.add(server);
        this.measureDistance = (measureDistance != null) ? measureDistance :
        	new Func1<String, TServerDistanceInfo>() { @Override public TServerDistanceInfo call(String server) { return measureDistance(server); }};
        this.initAction = (initAction != null) ? initAction : 
        	new Action1<Iterable<String>>() { @Override public void call(Iterable<String> servers) { init(servers); }};
        serverInfo = new HashMap<String, TServerDistanceInfo>();
    }

 	public ServerDistance(Iterable<String> servers, Func1<String, TServerDistanceInfo> measureDistance) { this(servers, measureDistance, null); }
 	public ServerDistance(Iterable<String> servers) { this(servers, null, null); }
 	
    public void start()
    {
    	new AsyncTask(new Action0() {
    		@Override public void call() { StartAsync(); }
		}).Start();
    }

    private void StartAsync()
    {
        initAction.call(servers);
        synchronized (this) {
        	serverInfo = new HashMap<String, TServerDistanceInfo>();
		}
        // Poll each server immediately, then with an Interval millisecond gap, each on task pool threads.
        // Note that Generate runs measureDistance immediately, then waits for the timeSelector interval before passing on the result and immediately running measureDistance again. In
        // order to get results quickly timeSelector is zero first time through, then a longer gap, but this means you get two measureDistances close together initially.
        // We use the TaskPool Scheduler, as the observables don't seem to shut down on disposal if we use NewThread.
        ArrayList<Observable<TServerDistanceInfo>> observables = new ArrayList<Observable<TServerDistanceInfo>>();
        for (String server : servers) {
			observables.add((new ObservableGenerator<TServerDistanceInfo>(10, interval, new DistanceMeasurer(server), NewThreadScheduler.getInstance())).
					getObservable());
		}
        poller = Observable.merge(observables).subscribe(onDistanceInfo);
    }

    public Action1<TServerDistanceInfo> onDistanceInfo = new Action1<TServerDistanceInfo>() {
		@Override
		public void call(TServerDistanceInfo sdi) {
	        // Ensure that multiple threads do not attempt to update ServerInfo at the same time.
	        synchronized (this) {
				serverInfo.put(sdi.name, sdi);
	            subject.onNext(sdi);
	            // Raise the FinishedInitialisation event just once, when all servers have been polled
	            if (!initialisedRaised && serverInfo.size() == servers.size()) {
	                finishedInitialisation.raise(this);
	                finishedInitialisation.removeAllHandlers();
	                initialisedRaised = true;
	            }
			}
		}
	};

    private EventNoData finishedInitialisation = new EventNoData();
    public void addFinishedInitialisationHandler(EventHandlerNoData handler) { finishedInitialisation.addHandler(handler); }
    public void removeFinishedInitialisationHandler(EventHandlerNoData handler) { finishedInitialisation.removeHandler(handler); }

    /**
     * Function to measure the distance to a given server. Must be supplied in the constructor or overridden
     */
    @SuppressWarnings("unchecked")
	protected TServerDistanceInfo measureDistance(String server)
    {
    	return (TServerDistanceInfo) new ServerDistanceInfo(server, false, Long.MAX_VALUE);
    }

    /**
     * Function to initialise the class. Does nothing by default.
     */
    protected void init(Iterable<String> servers) {}
    
	public void close() {
		poller.unsubscribe();
	}
}
