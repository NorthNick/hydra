package uk.co.shastra.hydra.messaging.utils;

import static org.junit.Assert.*;

import java.util.ArrayList;
import java.util.Date;

import org.junit.Ignore;
import org.junit.Test;

import rx.Subscription;
import rx.concurrency.NewThreadScheduler;
import rx.util.functions.Action1;
import rx.util.functions.Func0;

public class ObservableGeneratorTest {

	// This has to be a class vAariable to make it accessible to anonymous classes
	private ArrayList<Date> values;
	
	@Ignore("Some odd timing thing on Travis makes this test sometimes return 19 instead of 20 elements")
	@Test
	public void testGetObservable() {
		values = new ArrayList<Date>();
		// Returns current date
		Func0<Date> valueGenerator = new Func0<Date>() {
			@Override public Date call() {return new Date();}
		};
		// Generate values after 50ms and then every 100ms
		ObservableGenerator<Date> generator = new ObservableGenerator<Date>(50, 100, valueGenerator, NewThreadScheduler.getInstance());
		// Append generated values to the "values" ArrayList
		Subscription sub = generator.getObservable().subscribe(new Action1<Date>() {
			@Override public void call(Date date) {values.add(date);}
		});
		// Wait for n values to be generated
		int n = 20;
		try {
			Thread.sleep(100 * n);
			sub.unsubscribe();
			assertEquals(String.format("Should generate %1$s values during subscription", n), n, values.size());	
			Thread.sleep(200);
			assertEquals("Should generate no more values after subscription", n, values.size());	
		} catch (InterruptedException e) {
			// Auto-generated catch block
			e.printStackTrace();
		} finally {
			generator.close();
		}

	}

}
