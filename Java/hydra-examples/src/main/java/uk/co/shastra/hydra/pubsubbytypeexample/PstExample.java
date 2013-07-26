package uk.co.shastra.hydra.pubsubbytypeexample;

import java.awt.*;
import java.awt.event.*;
import java.io.FileInputStream;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.Properties;

import javax.swing.*;

import rx.util.functions.Action1;
import uk.co.shastra.hydra.messaging.HydraService;
import uk.co.shastra.hydra.messaging.listeners.ListenerOptions;
import uk.co.shastra.hydra.messaging.storage.NearestServerProvider;
import uk.co.shastra.hydra.pubsubbytype.Publisher;
import uk.co.shastra.hydra.pubsubbytype.Subscriber;

public class PstExample {

	private final String dateFormat = "yyyy-MM-dd HH:mm";
    private Publisher<PstMessage> publisher;
    private Subscriber<PstMessage> subscriber;
    private HydraService hydraService;
	private JTextField stringBox;
	private JTextField longBox;
	private JTextField dateBox;
	private SimpleDateFormat dateFormatter;
    
	public PstExample()
	{
		try {
			// Read config and set up Hydra servive
			Properties config = new Properties();
			// TODO - sort out path
			config.load(new FileInputStream("config.properties"));
            String pollIntervalMsSetting = config.getProperty("PollIntervalMs");
            Long pollIntervalMs = pollIntervalMsSetting == null ? null : Long.parseLong(pollIntervalMsSetting);
            ArrayList<String> servers = new ArrayList<String>();
            for (String server : config.getProperty("HydraServers").split(",")) {
            	servers.add(server.trim());
            }
            String portSetting = config.getProperty("Port");
            Integer port = portSetting == null ? null : Integer.parseInt(portSetting);

            hydraService = new HydraService(new NearestServerProvider(servers, config.getProperty("Database"), port), new ListenerOptions(0L, pollIntervalMs));
		} catch (Exception ex) {
			JOptionPane.showMessageDialog(null, "Error initialising: " + ex.getMessage(), "Error!", JOptionPane.ERROR_MESSAGE);
			return;
		}
		
		// Set up publisher and subscriber
        publisher = new Publisher<PstMessage>(hydraService);
        subscriber = new Subscriber<PstMessage>(hydraService, PstMessage.class);
        subscriber.getObservable().subscribe(new Action1<PstMessage>() {
			@Override public void call(PstMessage message) { onMessage(message); }
		});
        
		createUi();
	}
	
	private void send() {
        try {
            String stringField = stringBox.getText();
            long longField = Long.parseLong(longBox.getText());
            Date dateField = dateFormatter.parse(dateBox.getText(), new java.text.ParsePosition(0));
            publisher.send(new PstMessage(stringField, longField, dateField));
        } catch (Exception ex) {
        	JOptionPane.showMessageDialog(null, "Error sending message: " + ex.getMessage(), "Error!", JOptionPane.ERROR_MESSAGE);
        }
	}

    private void onMessage(PstMessage message)
    {
    	JOptionPane.showMessageDialog(null, "Received message:\n" + message, "Received message", JOptionPane.INFORMATION_MESSAGE);
    }
    
	private void createUi() {
		// Set up frame
		JFrame frame = new JFrame("PubSubByType Example");
		frame.setLayout(new GridBagLayout());
		frame.setSize(300, 200);
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		
		// Add some input fields
		int row = 0;
		frame.add(new JLabel("String:"), labelConstraints(row, 0));
		stringBox = new JTextField(20);
		frame.add(stringBox, textConstraints(row, 1));
		
		row++;
		frame.add(new JLabel("Long:"), labelConstraints(row, 0));
		longBox = new JTextField(20);
		frame.add(longBox, textConstraints(row, 1));
		
		// Set default date to now, to show user the input format
		row++;
		frame.add(new JLabel("Date:"), labelConstraints(row, 0));
		dateBox = new JTextField(20);
		frame.add(dateBox, textConstraints(row, 1));
		dateFormatter = new SimpleDateFormat(dateFormat);
		dateBox.setText(dateFormatter.format(new Date()));
		
		// Button to send the message to Hydra
		row++;
		JButton sendBtn = new JButton("Send");
		sendBtn.addActionListener(new ActionListener() {
			@Override public void actionPerformed(ActionEvent e) { send(); }
		});
		frame.add(sendBtn, buttonConstraints(row, 1));
		
		// Show the frame
		frame.setVisible(true);
	}
	
	private GridBagConstraints labelConstraints(int row, int col) {
		GridBagConstraints c = positionConstraints(row, col); 
		c.anchor = GridBagConstraints.LINE_END;
		c.insets = new Insets(3, 10, 3, 5);
		c.weightx = 0;
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

	public static void main(String args[])
	{
		SwingUtilities.invokeLater(new Runnable() {
			@Override public void run() { new PstExample(); }
		});
	}
	
}
