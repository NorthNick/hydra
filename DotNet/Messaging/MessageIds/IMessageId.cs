using System;

namespace Bollywell.Hydra.Messaging.MessageIds
{
    // Note that implementations of IMessageId should be private classes within MessageIdManager to ensure they are hidden.
    // See LongMessageId and UtcRandomMessageId for examples.
    public interface IMessageId : IComparable<IMessageId>, IEquatable<IMessageId>
    {
        string ToDocId();
        DateTime ToDateTime();
    }
}