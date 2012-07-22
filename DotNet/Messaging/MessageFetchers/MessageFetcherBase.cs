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

        protected abstract IKeyOptions MessageKey(IMessageId id);
        protected abstract IKeyOptions EndKey();

        #region Implementation of IMessageFetcher<TMessage>

        public IEnumerable<TMessage> MessagesAfterIdBeforeSeq(IDocumentDatabase db, IMessageId startId, long lastSeq)
        {
            return AllMessagesFrom(db, startId).Where(row => (long) row["value"] <= lastSeq).Select(TransportMessage.Hydrate<TMessage>)
                    .SkipWhile(message => message.MessageId.CompareTo(startId) <= 0);
        }

        public IEnumerable<TMessage> MessagesInSet(IDocumentDatabase db, IEnumerable<IMessageId> messageIds)
        {
            var options = new ViewOptions { IncludeDocs = true, Keys = messageIds.Select(MessageKey) };
            return db.View(ViewName, options, DesignDoc).Rows.Select(TransportMessage.Hydrate<TMessage>);
        }


        #endregion

        private IEnumerable<JToken> AllMessagesFrom(IDocumentDatabase db, IMessageId fromId)
        {
            // CouchDb startkey is inclusive, so this returns messages including fromId
            var options = new ViewOptions { IncludeDocs = true, StartKey = MessageKey(fromId), EndKey = EndKey() };
            return db.View(ViewName, options, DesignDoc).Rows;
        }

    }
}
