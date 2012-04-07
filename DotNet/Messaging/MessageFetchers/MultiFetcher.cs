using System.Collections.Generic;
using System.Linq;

namespace Bollywell.Hydra.Messaging.MessageFetchers
{
    public class MultiFetcher<TMessage> : IMessageFetcher<TMessage> where TMessage : TransportMessage
    {
        private readonly IMessageFetcher<TMessage>[] _fetchers;

        public MultiFetcher(params IMessageFetcher<TMessage>[] fetchers)
        {
            _fetchers = fetchers;
        }

        public List<TMessage> AllNewMessages()
        {
            return _fetchers.AsParallel().Select(f => f.AllNewMessages()).Merge().ToList();
        }

    }
}
