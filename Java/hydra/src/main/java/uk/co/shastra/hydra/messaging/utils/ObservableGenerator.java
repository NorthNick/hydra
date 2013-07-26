package uk.co.shastra.hydra.messaging.utils;

import java.util.concurrent.TimeUnit;

import rx.Observable;
import rx.Observer;
import rx.Scheduler;
import rx.Subscription;
import rx.subjects.PublishSubject;
import rx.util.functions.Action0;
import rx.util.functions.Func0;
import rx.util.functions.Func1;

public class ObservableGenerator<T> {
	
	private long waitMs;
	private long repeatMs;
	private Func0<T> valueGenerator;
	private Scheduler scheduler;
	private PublishSubject<T> subject = PublishSubject.create();
	private Observable<T> observable;
	private boolean started = false; 
	private boolean closed = false;
	
	private Action0 generatorAction = new Action0() {
		@Override public void call() {
			// Generate a value and schedule the timer to run again after the value is generated
			T value = valueGenerator.call();
			subject.onNext(value);
			if (!closed) {
				scheduler.schedule(generatorAction, repeatMs, TimeUnit.MILLISECONDS);
			}
		}};

	/**
	 * Generate an object that exposes an Observable generating a value after wait ms and then every repeat ms after the last value is finished.
	 * 
	 * Starts only when the first observer subscribes.
	 * 
	 * @param wait Number of ms until the first value generation starts. Must be > 1
	 * @param repeat Number of ms between end of one value generation and beginning of the next
	 * @param valueGenerator Function to generate values
	 * @param scheduler Scheduler to use for polling. Defaults to NewThreadScheduler.getInstance()
	 */
	public ObservableGenerator(long wait, long repeat, Func0<T> valueGenerator, Scheduler scheduler) {
		this.waitMs = Math.max(wait, 1);
		this.repeatMs = repeat;
		this.valueGenerator = valueGenerator;
		this.scheduler = scheduler;
		observable = Observable.create(new Func1<Observer<T>,Subscription>() {
				@Override
				public Subscription call(Observer<T> observer) {
					// This should really be locked in case of simultaneous subscriptions
					if (!started) {
						started = true;
						ObservableGenerator.this.scheduler.schedule(generatorAction, waitMs, TimeUnit.MILLISECONDS);
					}
					return subject.subscribe(observer);
				}			
			});
		}
	
	/**
	 * Get the Observable generated
	 * 
	 * @return The Observable
	 */
	public Observable<T> getObservable() {
		return observable;
	}
	
	public void close() {
		// prevent any more tasks running on the timer.
		closed = true;
		subject.onCompleted();
	}
}
