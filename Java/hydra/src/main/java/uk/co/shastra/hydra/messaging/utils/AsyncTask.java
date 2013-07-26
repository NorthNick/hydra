package uk.co.shastra.hydra.messaging.utils;

import rx.util.functions.Action0;

public class AsyncTask {

	private Action0 action;
	
	public AsyncTask(Action0 action) {
		this.action = action;
	}
	
	public void Start() {
		Thread t = new Thread(new Runnable() {
			@Override public void run() { action.call(); }
		});
		t.start();
	}
}
