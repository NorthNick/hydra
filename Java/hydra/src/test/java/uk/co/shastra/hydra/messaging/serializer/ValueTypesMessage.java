package uk.co.shastra.hydra.messaging.serializer;

class ValueTypesMessage {
	private int intField;
	private long longField;
	private boolean boolField;
	private float floatField;
	private double doubleField;
	private char charField;

	public int getIntField() {
		return intField;
	}
	public void setIntField(int intField) {
		this.intField = intField;
	}
	public long getLongField() {
		return longField;
	}
	public void setLongField(long longField) {
		this.longField = longField;
	}
	public boolean isBoolField() {
		return boolField;
	}
	public void setBoolField(boolean boolField) {
		this.boolField = boolField;
	}
	public float getFloatField() {
		return floatField;
	}
	public void setFloatField(float floatField) {
		this.floatField = floatField;
	}
	public double getDoubleField() {
		return doubleField;
	}
	public void setDoubleField(double doubleField) {
		this.doubleField = doubleField;
	}
	public char getCharField() {
		return charField;
	}
	public void setCharField(char charField) {
		this.charField = charField;
	}
	
	public ValueTypesMessage() {}
	
	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + (boolField ? 1231 : 1237);
		result = prime * result + charField;
		long temp;
		temp = Double.doubleToLongBits(doubleField);
		result = prime * result + (int) (temp ^ (temp >>> 32));
		result = prime * result + Float.floatToIntBits(floatField);
		result = prime * result + intField;
		result = prime * result + (int) (longField ^ (longField >>> 32));
		return result;
	}
	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		ValueTypesMessage other = (ValueTypesMessage) obj;
		if (boolField != other.boolField)
			return false;
		if (charField != other.charField)
			return false;
		if (Double.doubleToLongBits(doubleField) != Double
				.doubleToLongBits(other.doubleField))
			return false;
		if (Float.floatToIntBits(floatField) != Float
				.floatToIntBits(other.floatField))
			return false;
		if (intField != other.intField)
			return false;
		if (longField != other.longField)
			return false;
		return true;
	}
	
}