package uk.co.shastra.hydra.messaging.mocks;

import uk.co.shastra.hydra.messaging.storage.Provider;
import uk.co.shastra.hydra.messaging.storage.Store;

public class MockProvider implements Provider {

    private Store store;
	private String hydraServer;
	private Integer pollIntervalMs;

	public MockProvider(Store store) { this(store, null); }
	public MockProvider(Store store, Integer pollIntervalMs)
    {
        this.store = store;
        this.pollIntervalMs = pollIntervalMs;
        hydraServer = store.getName();
    }
    
	@Override
	public Store getStore(boolean waitForInitialisation) { return store; }

	@Override
	public String getHydraServer() { return hydraServer; }

	public Integer getPollIntervalMs() { return pollIntervalMs; }
	
	@Override
	public void serverError(String server) {
		// Not implemented
	}

}
