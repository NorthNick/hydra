using System.Collections.Generic;
using System.Linq;
using Bollywell.Hydra.Messaging.MessageIds;
using Bollywell.Hydra.Messaging.Storage;
using LoveSeat;
using LoveSeat.Interfaces;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public abstract class MessageFetcherBase<TMessage> : IMessageFetcher<TMessage> where TMessage : TransportMessage
    {
        protected abstract string ViewName { get; }

        protected abstract IKeyOptions MessageKey(IMessageId id);
        protected abstract IKeyOptions EndKey();

        #region Implementation of IMessageFetcher<TMessage>

        public IEnumerable<TMessage> MessagesAfterIdUpToSeq(IStore store, IMessageId startId, long lastSeq)
        {
            return AllMessagesFrom(store, startId).Where(row => (long) row["value"] <= lastSeq).Select(TransportMessage.Hydrate<TMessage>)
                    .SkipWhile(message => message.MessageId.CompareTo(startId) <= 0);
        }

        public IEnumerable<TMessage> MessagesInSet(IStore store, IEnumerable<IMessageId> messageIds)
        {
            var options = new ViewOptions { IncludeDocs = true, Keys = messageIds.Select(MessageKey) };
            return store.GetDocs(ViewName, options).Select(TransportMessage.Hydrate<TMessage>);
        }


        #endregion

        private IEnumerable<JToken> AllMessagesFrom(IStore store, IMessageId fromId)
        {
            // CouchDb startkey is inclusive, so this returns messages including fromId
            var options = new ViewOptions { IncludeDocs = true, StartKey = MessageKey(fromId), EndKey = EndKey() };
            return store.GetDocs(ViewName, options);
        }

    }
}
