package uk.co.shastra.hydra.messaging.serializer;

import java.util.ArrayList;

public class ArrayListMessage {
	private ArrayList<Boolean> arrayListField;

	public ArrayList<Boolean> getArrayListField() {
		return arrayListField;
	}

	public void setArrayListField(ArrayList<Boolean> arrayListField) {
		this.arrayListField = arrayListField;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result
				+ ((arrayListField == null) ? 0 : arrayListField.hashCode());
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
		ArrayListMessage other = (ArrayListMessage) obj;
		if (arrayListField == null) {
			if (other.arrayListField != null)
				return false;
		} else if (!arrayListField.equals(other.arrayListField))
			return false;
		return true;
	}
	
}
