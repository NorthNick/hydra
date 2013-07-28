package uk.co.shastra.hydra.messaging.serializer;

import java.util.ArrayList;
import java.util.Arrays;

import org.junit.Test;

import com.fasterxml.jackson.core.type.TypeReference;

import uk.co.shastra.hydra.messaging.serializers.HydraJsonSerializer;

public class SerializerGenericTest {

	@Test
	public void testArrayListMessage() {
		ArrayListMessage message = new ArrayListMessage();
		message.setArrayListField(new ArrayList<Boolean>(Arrays.asList(true, false, false, true)));
		HydraJsonSerializer<ArrayListMessage> serialiser = new HydraJsonSerializer<ArrayListMessage>(ArrayListMessage.class);
		SerializerNonGenericTest.checkRoundtrip(message, serialiser);
	}

	@Test
	public void testGenericMessage() {
		GenericMessage<ValueTypesMessage> message = new GenericMessage<ValueTypesMessage>();
		message.settField(new ValueTypesMessage());
		HydraJsonSerializer<GenericMessage<ValueTypesMessage>> serialiser = 
				new HydraJsonSerializer<GenericMessage<ValueTypesMessage>>(new TypeReference<GenericMessage<ValueTypesMessage>>() {});
		SerializerNonGenericTest.checkRoundtrip(message, serialiser);
	}
	
	@Test
	public void testWrappedGenericMessage() {
		WrappedGenericMessage message = new WrappedGenericMessage();
		GenericMessage<Integer> innermessage = new GenericMessage<Integer>();
		innermessage.settField(999);
		message.setGenericField(innermessage);
		HydraJsonSerializer<WrappedGenericMessage> serialiser = new HydraJsonSerializer<WrappedGenericMessage>(WrappedGenericMessage.class);
		SerializerNonGenericTest.checkRoundtrip(message, serialiser);
	}
}
