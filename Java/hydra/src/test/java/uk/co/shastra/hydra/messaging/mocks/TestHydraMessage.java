package uk.co.shastra.hydra.messaging.mocks;

import java.util.Date;

import uk.co.shastra.hydra.messaging.HydraMessage;

/**
 * Extension of HydraMessage with testing-specific fields
 *
 */
public class TestHydraMessage extends HydraMessage {

	/**
     * Generate the DocId from this date instead of the current date.
     */
    private Date idDate;
    public Date getIdDate() { return idDate; }
	public void setIdDate(Date idDate) { this.idDate = idDate; }

}
