using System;

namespace Bollywell.Hydra.Messaging.MessageIds
{
    /// <summary>
    /// Methods for manipulating IMessageIds.
    /// </summary>
    public static partial class MessageIdManager
    {
        public static IMessageId Create(string couchId)
        {
            return new LongMessageId(couchId);
        }

        public static IMessageId Create(DateTime utcDate)
        {
            return new LongMessageId(utcDate);
        }

        public static bool IsMessageId(string couchId)
        {
            return LongMessageId.IsMessageId(couchId);
        }

    }
}
