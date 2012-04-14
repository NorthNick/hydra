using System.Collections.Generic;
using System.Linq;
using LoveSeat;
using LoveSeat.Interfaces;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public abstract class MessageFetcherBase<TMessage> : IMessageFetcher<TMessage> where TMessage : TransportMessage
    {
        protected abstract string DesignDoc { get; }
        protected abstract string ViewName { get; }

        protected abstract IKeyOptions EndKey();
        protected abstract IKeyOptions MessageKey(IMessageId id);

        #region Implementation of IMessageFetcher<TMessage>

        public IEnumerable<TMessage> MessagesFromIdBeforeSeq(IMessageId startId, long lastSeq)
        {
            return AllMessagesFrom(startId).Where(row => (long) row["value"] <= lastSeq).Select(TransportMessage.Hydrate<TMessage>);
        }

        public IEnumerable<TMessage> MessagesInSet(IEnumerable<IMessageId> messageIds)
        {
            var options = new ViewOptions { IncludeDocs = true, Keys = messageIds.Select(MessageKey) };
            return Services.GetDb().View(ViewName, options, DesignDoc).Rows.Select(TransportMessage.Hydrate<TMessage>);
        }


        #endregion

        private IEnumerable<JToken> AllMessagesFrom(IMessageId fromId)
        {
            // The mceDirectedMessages view is indexed on [topic, destination, id]
            var options = new ViewOptions { IncludeDocs = true, StartKey = MessageKey(fromId), EndKey = EndKey() };
            return Services.GetDb().View(ViewName, options, DesignDoc).Rows;
        }

    }
}
