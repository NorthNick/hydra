using System.Collections.Generic;
using System.Linq;
using LoveSeat;
using LoveSeat.Interfaces;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public abstract class MessageFetcherBase<TMessage> : IMessageFetcher<TMessage> where TMessage : TransportMessage
    {
        protected abstract IKeyOptions MessageKey(IMessageId id);
        protected abstract IKeyOptions EndKey();

        protected abstract IEnumerable<JToken> DocRows(CouchDatabase db, ViewOptions options);

        #region Implementation of IMessageFetcher<TMessage>

        public IEnumerable<TMessage> MessagesAfterIdBeforeSeq(CouchDatabase db, IMessageId startId, long lastSeq)
        {
            // CouchDb startkey is inclusive, so we have to throw away startId if it is returned
            var options = new ViewOptions { IncludeDocs = true, StartKey = MessageKey(startId), EndKey = EndKey() };
            return DocRows(db, options).Where(row => (long) row["value"] <= lastSeq).Select(TransportMessage.Hydrate<TMessage>)
                    .SkipWhile(message => message.MessageId == startId);
        }

        public IEnumerable<TMessage> MessagesInSet(CouchDatabase db, IEnumerable<IMessageId> messageIds)
        {
            var options = new ViewOptions { IncludeDocs = true, Keys = messageIds.Select(MessageKey) };
            return DocRows(db, options).Select(TransportMessage.Hydrate<TMessage>);
        }

        #endregion

    }
}
