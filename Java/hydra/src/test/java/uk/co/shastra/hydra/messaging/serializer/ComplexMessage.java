package uk.co.shastra.hydra.messaging.serializer;

public class ComplexMessage {
	private StringMessage stringMessageField;
	private ValueTypesMessage valueTypesMessage;
	public StringMessage getStringMessageField() {
		return stringMessageField;
	}
	public void setStringMessageField(StringMessage stringMessageField) {
		this.stringMessageField = stringMessageField;
	}
	public ValueTypesMessage getValueTypesMessage() {
		return valueTypesMessage;
	}
	public void setValueTypesMessage(ValueTypesMessage valueTypesMessage) {
		this.valueTypesMessage = valueTypesMessage;
	}
	
	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime
				* result
				+ ((stringMessageField == null) ? 0 : stringMessageField
						.hashCode());
		result = prime
				* result
				+ ((valueTypesMessage == null) ? 0 : valueTypesMessage
						.hashCode());
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
		ComplexMessage other = (ComplexMessage) obj;
		if (stringMessageField == null) {
			if (other.stringMessageField != null)
				return false;
		} else if (!stringMessageField.equals(other.stringMessageField))
			return false;
		if (valueTypesMessage == null) {
			if (other.valueTypesMessage != null)
				return false;
		} else if (!valueTypesMessage.equals(other.valueTypesMessage))
			return false;
		return true;
	}
	
}
