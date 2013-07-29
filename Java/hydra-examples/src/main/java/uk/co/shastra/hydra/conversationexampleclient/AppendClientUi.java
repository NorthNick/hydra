package uk.co.shastra.hydra.conversationexampleclient;

import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.Insets;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;

import javax.swing.JButton;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JTextField;

import rx.Subscription;
import rx.util.functions.Action1;
import uk.co.shastra.hydra.conversationexampledto.ConversationDto;
import uk.co.shastra.hydra.conversationexampledto.MessageTypes;
import uk.co.shastra.hydra.conversations.Conversation;
import uk.co.shastra.hydra.messaging.utils.ObservableUtils;

public class AppendClientUi extends JPanel {

	private static final long serialVersionUID = 5627349470260313288L;
    private Conversation<ConversationDto> conversation;
    private Subscription subscription;
	private JTextField requestBox;
	private JLabel handleLbl;
	private JLabel suffixLbl;
	private JLabel responseLbl;
    
    public AppendClientUi()
    {
    	super();
    	createUi();
    }

	public void init(Conversation<ConversationDto> conversation, String suffix) {
		handleLbl.setText(handleLbl.getText() + " " + conversation.getHandle());
		suffixLbl.setText(suffixLbl.getText() + " " + suffix);

        this.conversation = conversation;
        subscription = ObservableUtils.skipErrors(conversation.getObservable()).subscribe(new Action1<ConversationDto>() {
			@Override public void call(ConversationDto dto) { onNext(dto); }
		});
        
        ConversationDto dto = new ConversationDto();
        dto.setMessageType(MessageTypes.INIT);
        dto.setData(suffix);
        try {
			conversation.send(dto);
		} catch (Exception e) {}
	}

    private void onNext(ConversationDto message)
    {
        responseLbl.setText(String.format("Last response: %1$s, %2$s", message.getMessageType(), message.getData()));
    }
    
	private void createUi() {
		setLayout(new GridBagLayout());
		setSize(250, 100);

		// Labels
		int row = 0;
		handleLbl = addLabelledValue(row, "Handle:");
		row++;
		suffixLbl = addLabelledValue(row, "Suffix:");	
		row++;
		responseLbl = addLabelledValue(row, "Last response:");
		row++;
		add(new JLabel("Request:"), labelConstraints(row, 0));
		requestBox = new JTextField(20);
		add(requestBox, textConstraints(row, 1));
		
		// Request button
		row++;
		JButton requestBtn = new JButton("Request");
		add(requestBtn, buttonConstraints(row, 0));
		requestBtn.addActionListener(new ActionListener() {
			@Override public void actionPerformed(ActionEvent ae) { requestBtnClick(ae); }
		});
				
		// End button
		JButton endBtn = new JButton("End");
		add(endBtn, buttonConstraints(row, 1));
		endBtn.addActionListener(new ActionListener() {
			@Override public void actionPerformed(ActionEvent ae) { endBtnClick(ae); }
		});
		
		setVisible(true);
	}

	private void requestBtnClick(ActionEvent ae) {
		ConversationDto dto = new ConversationDto();
		dto.setMessageType(MessageTypes.REQUEST);
		dto.setData(requestBox.getText());
		try {
			conversation.send(dto);
		} catch (Exception e) {}
	}
	
	protected void endBtnClick(ActionEvent ae) {
		ConversationDto dto = new ConversationDto();
		dto.setMessageType(MessageTypes.END);
        try {
			conversation.send(dto);
		} catch (Exception e) {}
        subscription.unsubscribe();
        conversation.close();;
	}
	
	private JLabel addLabelledValue(int row, String label) {
		add(new JLabel(label), labelConstraints(row, 0));
		JLabel res = new JLabel();
		add(res, valueConstraints(row, 1));
		return res;
	}
	
	private GridBagConstraints labelConstraints(int row, int col) {
		GridBagConstraints c = positionConstraints(row, col); 
		c.anchor = GridBagConstraints.LINE_END;
		c.insets = new Insets(3, 10, 3, 5);
		c.weightx = 0;
		return c;
	}
	
	private GridBagConstraints valueConstraints(int row, int col) {
		GridBagConstraints c = positionConstraints(row, col); 
		c.anchor = GridBagConstraints.LINE_START;
		c.insets = new Insets(3, 10, 3, 5);
		c.weightx = 1;
		return c;
	}
	
	private GridBagConstraints textConstraints(int row, int col) {
		GridBagConstraints c = positionConstraints(row, col);
		c.fill = GridBagConstraints.HORIZONTAL;
		c.insets = new Insets(3, 5, 3, 10);
		c.weightx = 1;
		return c;
	}

	private GridBagConstraints buttonConstraints(int row, int col) {
		GridBagConstraints c = positionConstraints(row, col);
		c.anchor = GridBagConstraints.LINE_END;
		c.insets = new Insets(3, 5, 3, 10);
		return c;
	}
	
	private GridBagConstraints positionConstraints(int row, int col) {
		GridBagConstraints c = new GridBagConstraints();
		c.gridx = col;
		c.gridy = row;
		return c;
	}
}
