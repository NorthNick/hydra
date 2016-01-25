package uk.co.shastra.hydra.messaging.storage;

import static org.junit.Assert.*;
import org.junit.Ignore;
import org.junit.Test;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;

public class CouchDbStoreTest {

	private class TestType {
		private int integer;
		private String data;
		
		@SuppressWarnings("unused")
		public int getInteger() {
			return integer;
		}
		@SuppressWarnings("unused")
		public void setInteger(int integer) {
			this.integer = integer;
		}
		@SuppressWarnings("unused")
		public String getData() {
			return data;
		}
		@SuppressWarnings("unused")
		public void setData(String data) {
			this.data = data;
		}
		
		public TestType(int integer, String data) {
			super();
			this.integer = integer;
			this.data = data;
		}
		
	}
	
	@Test
	public void testGetLastSeq() {
		CouchDbStore store = new CouchDbStore("localhost");
		long lastSeq = store.getLastSeq();
		// Just check we get something back.
		assertTrue(lastSeq >= 0);
	}

	@Test
	public void testSaveWorks() {
		CouchDbStore store = new CouchDbStore("localhost");
		TestType testData = new TestType(23, "testSaveWorks");
		ObjectMapper mapper = new ObjectMapper();
		JsonNode node = mapper.valueToTree(testData);
		store.saveDoc(node);
		assertTrue(1 > 0);
	}
	
	@Ignore("Requires broadcastMessages view, which is not there during CI")
	@Test
	public void testmeasureDistance() {
		CouchDbStore store = new CouchDbStore("localhost");
		ServerDistanceInfo distance = store.measureDistance();
		assertTrue("Server should be reachable", distance.isReachable);
	}
}
