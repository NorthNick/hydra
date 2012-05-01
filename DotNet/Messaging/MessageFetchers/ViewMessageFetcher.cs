using System.Collections.Generic;
using LoveSeat;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public abstract class ViewMessageFetcher<TMessage> : MessageFetcherBase<TMessage> where TMessage : TransportMessage
    {
        protected abstract string DesignDoc { get; }
        protected abstract string ViewName { get; }

        protected override IEnumerable<JToken> DocRows(CouchDatabase db, ViewOptions options)
        {
            return db.View(ViewName, options, DesignDoc).Rows;
        }

    }
}
