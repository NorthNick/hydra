package uk.co.shastra.hydra.messaging.serializer;

import static org.junit.Assert.*;

import org.junit.Test;

import com.fasterxml.jackson.core.type.TypeReference;

import uk.co.shastra.hydra.messaging.serializers.HydraJsonSerializer;
import uk.co.shastra.hydra.messaging.serializers.Serializer;

public class SerializerNonGenericTest {

	// Note the use of "static" - this makes it possible for Jackson to deserialise the nested type.
	// See http://cowtowncoder.com/blog/archives/2010/08/entry_411.html
	@SuppressWarnings("unused")
	private static class NestedTypeMessage {
		private int intfield;

		public int getIntfield() {
			return intfield;
		}

		public void setIntfield(int intfield) {
			this.intfield = intfield;
		}

		@Override
		public boolean equals(Object obj) {
			if (this == obj)
				return true;
			if (obj == null)
				return false;
			if (getClass() != obj.getClass())
				return false;
			NestedTypeMessage other = (NestedTypeMessage) obj;
			if (intfield != other.intfield)
				return false;
			return true;
		}
	}
	
	@Test
	public void testEmptyMessage() {
		EmptyMessage message = new EmptyMessage();
		HydraJsonSerializer<EmptyMessage> serialiser = new HydraJsonSerializer<EmptyMessage>(EmptyMessage.class); 
		checkRoundtrip(message, serialiser);
	}
	
	@Test
	public void testNestedTypeMessage() {
		NestedTypeMessage message = new NestedTypeMessage();
		message.setIntfield(-56);
		HydraJsonSerializer<NestedTypeMessage> serialiser = new HydraJsonSerializer<NestedTypeMessage>(NestedTypeMessage.class);
		checkRoundtrip(message, serialiser);
	}
	
	@Test
	public void testStringMessage() {
		StringMessage message = new StringMessage();
		message.setStringField("Hello");
		HydraJsonSerializer<StringMessage> serialiser = new HydraJsonSerializer<StringMessage>(StringMessage.class);
		checkRoundtrip(message, serialiser);
		
	}
	
	@Test
	public void testValueTypesMessage() {
		ValueTypesMessage message = new ValueTypesMessage();
		message.setBoolField(true);
		message.setCharField('\n');
		message.setDoubleField(4.3321);
		message.setFloatField(5678.6F);
		message.setIntField(796);
		message.setLongField(8909805L);		
		HydraJsonSerializer<ValueTypesMessage> serialiser = new HydraJsonSerializer<ValueTypesMessage>(ValueTypesMessage.class);
		checkRoundtrip(message, serialiser);
	}

	@Test
	public void testArrayMessage() {
		ArrayMessage message = new ArrayMessage();
		message.setIntArrayField(new int[] {10, 45, 6, 111});
		HydraJsonSerializer<ArrayMessage> serialiser = new HydraJsonSerializer<ArrayMessage>(ArrayMessage.class);
		checkRoundtrip(message, serialiser);
	}
	
	@Test
	public void testComplexMessage() {
		StringMessage sMessage = new StringMessage();
		sMessage.setStringField("Some other string");
		// Use default field values for vtMessage
		ValueTypesMessage vtMessage = new ValueTypesMessage();
		ComplexMessage message = new ComplexMessage();
		message.setStringMessageField(sMessage);
		message.setValueTypesMessage(vtMessage);
		HydraJsonSerializer<ComplexMessage> serialiser = new HydraJsonSerializer<ComplexMessage>(ComplexMessage.class);
		checkRoundtrip(message, serialiser);
	}
	
	@Test
	public void testNullFields() {
		ComplexMessage message = new ComplexMessage();
		HydraJsonSerializer<ComplexMessage> serialiser = new HydraJsonSerializer<ComplexMessage>(ComplexMessage.class);
		String json = serialiser.serialize(message);
		assertEquals("ComplexMessage with null fields should serialise to empty JSON", "{}", json);
		checkRoundtrip(message, serialiser);
		message.setStringMessageField(new StringMessage());
		json = serialiser.serialize(message);
		// Strip spaces from JSON as they are optional
		assertEquals("Should ignore valueTypesMessageField and serialise stringMessageField to the empty object",
				"{\"stringMessageField\":{}}", json.replace(" ", ""));
		checkRoundtrip(message, serialiser);
	}
	
	@Test
	public void testTypeRef() {
		StringMessage sMessage = new StringMessage();
		sMessage.setStringField("Some other string");
		// Use default field values for vtMessage
		ValueTypesMessage vtMessage = new ValueTypesMessage();
		ComplexMessage message = new ComplexMessage();
		message.setStringMessageField(sMessage);
		message.setValueTypesMessage(vtMessage);
		HydraJsonSerializer<ComplexMessage> serialiser = new HydraJsonSerializer<ComplexMessage>(new TypeReference<ComplexMessage>() {});
		checkRoundtrip(message, serialiser);
	}
	
	@Test
	public void testNullFieldsWithTyperef() {
		ComplexMessage message = new ComplexMessage();
		HydraJsonSerializer<ComplexMessage> serialiser = new HydraJsonSerializer<ComplexMessage>(new TypeReference<ComplexMessage>() {});
		String json = serialiser.serialize(message);
		assertEquals("ComplexMessage with null fields should serialise to empty JSON", "{}", json);
		checkRoundtrip(message, serialiser);
		message.setStringMessageField(new StringMessage());
		json = serialiser.serialize(message);
		// Strip spaces from JSON as they are optional
		assertEquals("Should ignore valueTypesMessageField and serialise stringMessageField to the empty object",
				"{\"stringMessageField\":{}}", json.replace(" ", ""));
		checkRoundtrip(message, serialiser);
	}
	
	public static <TMessage> void checkRoundtrip(TMessage message, Serializer<TMessage> serialiser)
	{
		String json = serialiser.serialize(message);
		assertNotNull("Serialisation should succeed", json);
		TMessage newMessage = null;
		try {
			newMessage = serialiser.deserialize(json);
		} catch (Exception e) {}
		assertNotNull("Deserialisation should succeed", newMessage);
		assertEquals("Deserialised object should be the same as the original", message, newMessage);
	}
}
