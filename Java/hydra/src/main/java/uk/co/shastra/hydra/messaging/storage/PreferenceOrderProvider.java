package uk.co.shastra.hydra.messaging.storage;

import java.util.ArrayList;
import java.util.HashMap;

import rx.Observable;
import rx.util.functions.Action1;

public class PreferenceOrderProvider extends PollingProviderBase {

    private ArrayList<String> servers = new ArrayList<String>();
    private HashMap<String, Integer> serverIndices = new HashMap<String, Integer>();
    private int preferredIndex;
    
    public PreferenceOrderProvider(Iterable<String> hydraServers, String database) { this(hydraServers, database, null); }
    public PreferenceOrderProvider(Iterable<String> hydraServers, String database, Integer port) { 
    	super(hydraServers, database, port);
    	init(hydraServers);
    }

	public PreferenceOrderProvider(Iterable<Store> stores) { 
    	super(stores);
    	ArrayList<String> hydraServers = new ArrayList<String>();
    	for (Store store : stores) {
			hydraServers.add(store.getName());
		}
    	init(hydraServers);
    }

    private void init(Iterable<String> hydraServers) {
    	int count = 0;
    	for (String server : hydraServers) {
    		servers.add(server);
			serverIndices.put(server, count);
		}
    	preferredIndex = count;
	}
    
	@Override
	protected void doSubscription(Observable<ServerDistanceInfo> observable) {
		observable.subscribe(new Action1<ServerDistanceInfo>() {
			@Override
			public void call(ServerDistanceInfo sdi) {
	            int sdiIndex = serverIndices.get(sdi.name);
	            if (sdi.isReachable && sdiIndex < preferredIndex && EarlierServersUnreachable(sdiIndex)) {
	                // A server is preferred if it is earlier in servers than the current server, it is reachable, and all servers before it in servers are unreachable.
	                preferredIndex = sdiIndex;
	                setHydraServer(sdi.name);
	            } else if (!sdi.isReachable && sdiIndex == preferredIndex) {
	                // The current server is no longer responding - replace with the first reachable one before which all others are unreachable, or else null
	            	String server = null;
	                int ii = 0;
	                while (ii < servers.size()) {
	                	String candidate = servers.get(ii);
	                	if (distances.getServerInfo().containsKey(candidate) && !distances.getServerInfo().get(candidate).isReachable) {
	                		ii++;
	                	} else {
	                		server = candidate;
	                		break;
	                	}
					}
	                setHydraServer(server != null && distances.getServerInfo().containsKey(server) ? server : null);
	                preferredIndex = getHydraServer() == null ? servers.size() : serverIndices.get(getHydraServer());
	            } else if (!initialised && !sdi.isReachable && distances.getServerInfo().size() == storeDict.size()) {
	                // All servers are unreachable so initialisation is done
	                finishedInitialisation();
	            }
			}

			/**
			 * Check whether all servers before sdiIndex are in distance.getServerInfo() and are unreachable
			 * 
			 * @param sdiIndex
			 * @return Whether the condition is true
			 */
			private boolean EarlierServersUnreachable(int sdiIndex) {
				for (int ii = 0; ii < sdiIndex; ii++) {
					String server = servers.get(ii);
					if (!distances.getServerInfo().containsKey(server) || distances.getServerInfo().get(server).isReachable)
						return false;
				}
				return true;
			}
		});
	}

}
