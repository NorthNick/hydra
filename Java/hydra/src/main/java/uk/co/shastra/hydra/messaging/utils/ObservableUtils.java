package uk.co.shastra.hydra.messaging.utils;

import rx.Notification;
import rx.Notification.Kind;
import rx.Observable;
import rx.util.functions.Func1;

public class ObservableUtils {
	
	public static <T> Observable<T> skipErrors(Observable<Notification<T>> source) {
		return source.where(new Func1<Notification<T>, Boolean>() {
			@Override public Boolean call(Notification<T> n) { return n.getKind() != Kind.OnError; }
		}).dematerialize();
	}
	
}
