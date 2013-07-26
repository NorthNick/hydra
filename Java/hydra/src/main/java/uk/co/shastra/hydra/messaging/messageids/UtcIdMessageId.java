package uk.co.shastra.hydra.messaging.messageids;

import java.util.Date;
import java.util.regex.Pattern;

class UtcIdMessageId implements MessageId {

	// A message id is 14 lower-case hex characters followed by a suffix. The 14
	// hex characters are microseconds since 1 Jan 1970 UTC; the last 18 are random.
	private static Pattern IdPattern = Pattern.compile("^[0-9a-f]{14}.*");

	private long timeBits;
	private String suffix;

    public UtcIdMessageId(String couchId)
    {
        timeBits = Long.parseLong(couchId.substring(0, 14), 16);
        suffix = couchId.substring(14);
    }

    public UtcIdMessageId(Date utcDate)
    {
        // Date only gives millisecond accuracy so multiply by 1000 to get microseconds.
        timeBits = utcDate.getTime() * 1000;
        suffix = "";
    }

	@Override
	public String toDocId() {
		// _timeBits is converted to a zero left-padded 14 character lower case hex string
		return String.format("%1$014x%2$s", timeBits, suffix);
	}

	@Override
	public Date toDateTime() {
		return new Date(timeBits / 1000);
	}

    public static boolean isMessageId(String couchId)
    {
        return couchId != null && IdPattern.matcher(couchId).matches();
    }
    
	@Override
	public int compareTo(MessageId other) {
		UtcIdMessageId utcOther = (UtcIdMessageId) other;
		// No compareTo for primitive types
		int highCompare = timeBits < utcOther.timeBits ? -1 : timeBits == utcOther.timeBits ? 0 : 1;
		return highCompare == 0 ? suffix.compareTo(utcOther.suffix) : highCompare;
	}

	@Override
	public boolean equals(Object o) {
		// Return true if the objects are identical.
		// (This is just an optimisation, not required for correctness.)
		if (this == o) {
			return true;
		}
		// Return false if the other object has the wrong type.
		if (!(o instanceof UtcIdMessageId)) {
			return false;
		}
		// Cast to the appropriate type.
		// This will succeed because of the instanceof, and lets us access private fields.
		UtcIdMessageId utcOther = (UtcIdMessageId) o;
		return timeBits == utcOther.timeBits && suffix.equals(utcOther.suffix);
	}
	

    public String toString()
    {
        return String.format("%1$014x%2$s Time %3$s", timeBits, suffix, toDateTime().toString());
    }
}
