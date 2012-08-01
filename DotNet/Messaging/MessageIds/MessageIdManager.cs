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
            return new UtcRandomMessageId(couchId);
        }

        public static IMessageId Create(DateTime utcDate)
        {
            return new UtcRandomMessageId(utcDate);
        }

        public static bool IsMessageId(string couchId)
        {
            return UtcRandomMessageId.IsMessageId(couchId);
        }

    }
}
