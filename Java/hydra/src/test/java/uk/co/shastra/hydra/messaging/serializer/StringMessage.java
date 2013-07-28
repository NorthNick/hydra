package uk.co.shastra.hydra.messaging.serializer;

public class StringMessage {
	private String stringField;

	public String getStringField() {
		return stringField;
	}

	public void setStringField(String stringField) {
		this.stringField = stringField;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result
				+ ((stringField == null) ? 0 : stringField.hashCode());
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
		StringMessage other = (StringMessage) obj;
		if (stringField == null) {
			if (other.stringField != null)
				return false;
		} else if (!stringField.equals(other.stringField))
			return false;
		return true;
	}

}
