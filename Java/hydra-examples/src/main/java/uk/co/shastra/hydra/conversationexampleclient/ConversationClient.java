package uk.co.shastra.hydra.conversationexampleclient;

import java.awt.FlowLayout;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.util.ArrayList;
import java.util.Properties;

import javax.swing.JButton;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JTextField;
import javax.swing.SwingUtilities;

import uk.co.shastra.hydra.conversationexampledto.ConversationDto;
import uk.co.shastra.hydra.conversations.Conversation;
import uk.co.shastra.hydra.conversations.Switchboard;
import uk.co.shastra.hydra.messaging.StdHydraService;
import uk.co.shastra.hydra.messaging.listeners.ListenerOptions;
import uk.co.shastra.hydra.messaging.storage.NearestServerProvider;

public class ConversationClient {

    private final String MyName = "AppendClient";
    private Switchboard<ConversationDto> switchboard;
	private JFrame frame;
	private JTextField suffixBox;
    
	public ConversationClient()
	{
		try {
			// Read config and set up Hydra servive
			Properties config = new Properties();
			config.load(getClass().getResourceAsStream("/config.properties"));
            String pollIntervalMsSetting = config.getProperty("PollIntervalMs");
            Long pollIntervalMs = pollIntervalMsSetting == null ? null : Long.parseLong(pollIntervalMsSetting);
            ArrayList<String> servers = new ArrayList<String>();
            for (String server : config.getProperty("HydraServers").split(",")) {
            	servers.add(server.trim());
            }
            String portSetting = config.getProperty("Port");
            Integer port = portSetting == null ? null : Integer.parseInt(portSetting);

            StdHydraService hydraService = new StdHydraService(new NearestServerProvider(servers, config.getProperty("Database"), port), new ListenerOptions(0L, pollIntervalMs));
            
    		// Set up switchboard
    		switchboard = new Switchboard<ConversationDto>(hydraService, ConversationDto.class, MyName);
		} catch (Exception ex) {
			JOptionPane.showMessageDialog(null, "Error initialising: " + ex.getMessage(), "Error!", JOptionPane.ERROR_MESSAGE);
			return;
		}
		
		createUi();
	}
	
	private void createUi() {
		// Set up frame
		frame = new JFrame("Conversation Example Client");
		frame.setLayout(new FlowLayout());
		frame.setSize(350, 1000);
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		
		// Add some input fields
		frame.add(new JLabel("Suffix:"));
		suffixBox = new JTextField(20);
		frame.add(suffixBox);
		
		// New Client button
		JButton newBtn = new JButton("New Client");
		newBtn.addActionListener(new ActionListener() {
			@Override public void actionPerformed(ActionEvent ae) { newBtnClick(ae); }
		});
		frame.add(newBtn);
		
		// Show the frame
		frame.setVisible(true);
	}
	
    private void newBtnClick(ActionEvent ae)
    {
        Conversation<ConversationDto> client = switchboard.newConversation("AppendServer");
        AppendClientUi clientUi = new AppendClientUi();
        clientUi.init(client, suffixBox.getText());
        frame.add(clientUi);
        frame.validate();
        frame.repaint();
    }
    
	public static void main(String args[])
	{
		SwingUtilities.invokeLater(new Runnable() {
			@Override public void run() { new ConversationClient(); }
		});
	}
}
