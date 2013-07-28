package uk.co.shastra.hydra.messaging.serializer;

public class WrappedGenericMessage {
	private GenericMessage<Integer> genericField;

	public GenericMessage<Integer> getGenericField() {
		return genericField;
	}

	public void setGenericField(GenericMessage<Integer> genericField) {
		this.genericField = genericField;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result
				+ ((genericField == null) ? 0 : genericField.hashCode());
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
		WrappedGenericMessage other = (WrappedGenericMessage) obj;
		if (genericField == null) {
			if (other.genericField != null)
				return false;
		} else if (!genericField.equals(other.genericField))
			return false;
		return true;
	}
	
}
