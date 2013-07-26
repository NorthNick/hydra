package uk.co.shastra.hydra.messaging.utils;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.TreeSet;

public class ListUtils {

	public static <T> ArrayList<T> IterableToArrayList(Iterable<T> iterable) {
		Iterator<T> iterator = iterable.iterator();
		ArrayList<T> res = new ArrayList<T>();
		while (iterator.hasNext()) {
			res.add(iterator.next());
		}
		return res;
	}
	
	// This should be <T extends Comparable<T>> but then the use in Listener gives a compiler error.
	// The ? business is to allow parameters of type <ArrayList<ArrayList<Whatever>>
	public static <T> ArrayList<T> Merge(Iterable<? extends Iterable<T>> lists) {
		// Quick and dirty implementation for now
		TreeSet<T> sortedSet = new TreeSet<T>();
		for (Iterable<T> list : lists) {
			for (T t : list) {
				sortedSet.add(t);
			}
		}
		ArrayList<T> res = new ArrayList<T>();
		for (T t : sortedSet) {
			res.add(t);
		}
		return res;
	}
}
