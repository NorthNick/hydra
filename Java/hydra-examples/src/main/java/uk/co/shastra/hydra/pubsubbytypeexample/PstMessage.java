package uk.co.shastra.hydra.pubsubbytypeexample;

import java.util.Date;

public class PstMessage {
	private String stringField;
	private long longField;
	private Date dateField;
	
	public String getStringField() { return stringField; }
	public void setStringField(String stringField) { this.stringField = stringField; }
	public long getLongField() { return longField; }
	public void setLongField(long longField) { this.longField = longField; }
	public Date getDateField() { return dateField; }
	public void setDateField(Date dateField) { this.dateField = dateField; }
	
	// Constructor for use by the deserializer, which will set properties individually.
	public PstMessage() {}
	
	public PstMessage(String stringField, long longField, Date dateField) {
		this.stringField = stringField;
		this.longField = longField;
		this.dateField = dateField;
	}
	
	@Override
	public String toString() {
		return String.format("String: %1$s\nLong: %2$s\nDate: %3$s", stringField, longField, dateField);
	}
	
}
