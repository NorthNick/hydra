package uk.co.shastra.hydra.conversationexampleserver;

import java.awt.FlowLayout;
import java.io.FileInputStream;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.Properties;

import javax.swing.JFrame;
import javax.swing.JOptionPane;
import javax.swing.SwingUtilities;

import rx.util.functions.Action1;
import uk.co.shastra.hydra.conversationexampledto.ConversationDto;
import uk.co.shastra.hydra.conversations.Conversation;
import uk.co.shastra.hydra.conversations.Switchboard;
import uk.co.shastra.hydra.messaging.HydraService;
import uk.co.shastra.hydra.messaging.listeners.ListenerOptions;
import uk.co.shastra.hydra.messaging.storage.NearestServerProvider;

public class ConversationServer {

    private final String MyName = "AppendServer";
    private HashSet<AppendServer> servers = new HashSet<AppendServer>();

	public ConversationServer()
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

            HydraService hydraService = new HydraService(new NearestServerProvider(servers, config.getProperty("Database"), port), new ListenerOptions(0L, pollIntervalMs));
            
    		// Set up switchboard
    		new Switchboard<ConversationDto>(hydraService, ConversationDto.class, MyName).
    			getObservable().
    			subscribe(new Action1<Conversation<ConversationDto>>() {
	    			@Override public void call(Conversation<ConversationDto> conversation) { onNext(conversation); }
				});
		} catch (Exception ex) {
			JOptionPane.showMessageDialog(null, "Error initialising: " + ex.getMessage(), "Error!", JOptionPane.ERROR_MESSAGE);
			return;
		}
		
		createUi();
	}
	
	private void onNext(Conversation<ConversationDto> conversation)
	{
		servers.add(new AppendServer(conversation));
	}
	
	private void createUi() {
		// Set up empty frame
		JFrame frame = new JFrame("Conversation Example Server");
		frame.setLayout(new FlowLayout());
		frame.setSize(200, 200);
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		
		// Show the frame
		frame.setVisible(true);
	}
	
	public static void main(String args[])
	{
		SwingUtilities.invokeLater(new Runnable() {
			@Override public void run() { new ConversationServer(); }
		});
	}
}
