package uk.co.shastra.hydra.messaging.storage;

import static org.junit.Assert.*;

import java.util.ArrayList;
import java.util.Arrays;

import org.junit.Test;

import uk.co.shastra.hydra.messaging.utils.EventHandlerNoData;

public class ServerDistanceTest {

	@Test
	public void testConstructor() {
		ServerDistance<ServerDistanceInfo> distances = new ServerDistance<ServerDistanceInfo>(new ArrayList<String>(Arrays.asList("test")));
		assertEquals("ServerInfo should be empty initially", 0, distances.getServerInfo().size(), 0);
		long testInterval = 3000;
		distances.setInterval(testInterval );
		assertEquals("Should be able to change Interval", testInterval, distances.getInterval());
	}
	
	private boolean finishedInitialisationFired = false;
	
	@Test
	public void testStart() {
		ServerDistance<ServerDistanceInfo> distances = new ServerDistance<ServerDistanceInfo>(new ArrayList<String>(Arrays.asList("test")));
	    distances.addFinishedInitialisationHandler(new EventHandlerNoData() {
			@Override public void handle(Object source) { finishedInitialisationFired = true; }
		});
	    distances.start();
	    // Allow time for start to complete
	    try {
			Thread.sleep(100);
		} catch (InterruptedException e) {}
	    assertTrue("FinishedInitialisation event should have fired", finishedInitialisationFired);
	}

}
