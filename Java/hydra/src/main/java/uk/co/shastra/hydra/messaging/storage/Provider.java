package uk.co.shastra.hydra.messaging.storage;

public interface Provider {
    Store getStore(boolean waitForInitialisation);
    String getHydraServer();
    void serverError(String server);
}
