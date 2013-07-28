package uk.co.shastra.hydra.messaging.serializer;

import java.util.Arrays;

public class ArrayMessage {
	private int[] intArrayField;

	public int[] getIntArrayField() {
		return intArrayField;
	}

	public void setIntArrayField(int[] intArrayField) {
		this.intArrayField = intArrayField;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + Arrays.hashCode(intArrayField);
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
		ArrayMessage other = (ArrayMessage) obj;
		if (!Arrays.equals(intArrayField, other.intArrayField))
			return false;
		return true;
	}
}
