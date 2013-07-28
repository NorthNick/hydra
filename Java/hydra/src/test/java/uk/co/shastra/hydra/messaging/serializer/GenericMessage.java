package uk.co.shastra.hydra.messaging.serializer;

public class GenericMessage<T> {
	private T tField;

	public T gettField() {
		return tField;
	}

	public void settField(T tField) {
		this.tField = tField;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((tField == null) ? 0 : tField.hashCode());
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
		GenericMessage<?> other = (GenericMessage<?>) obj;
		if (tField == null) {
			if (other.tField != null)
				return false;
		} else if (!tField.equals(other.tField))
			return false;
		return true;
	}
	
}
