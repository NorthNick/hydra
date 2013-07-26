package uk.co.shastra.hydra.messaging.utils;

import static org.junit.Assert.*;

import java.util.ArrayList;
import java.util.Arrays;

import org.junit.Test;

public class ListUtilsTest {

	@SuppressWarnings("unchecked")
	@Test
	public void testMerge() {
		ArrayList<ArrayList<Integer>> arg = new ArrayList<ArrayList<Integer>>(Arrays.asList(
        		new ArrayList<Integer>(Arrays.asList(1, 3, 5)),
        		new ArrayList<Integer>(Arrays.asList(-1, 1, 2, 4)),
        		new ArrayList<Integer>(Arrays.asList(6, 7))));
		ArrayList<Integer> expected = new ArrayList<Integer>(Arrays.asList(-1, 1, 2, 3, 4, 5, 6, 7));
		ArrayList<Integer> res = ListUtils.Merge(arg);
		assertTrue(arraysEqual(expected, res));
	}
	
	private boolean arraysEqual(ArrayList<Integer> first, ArrayList<Integer> second) {
		if (first.size() != second.size()) return false;
		
		for (int ii = 0; ii < first.size(); ii++) {
			if (first.get(ii) != second.get(ii)) return false;
		}
		return true;
	}

}
