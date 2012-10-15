using Bollywell.Hydra.Messaging.Storage;
using System;

namespace Bollywell.Hydra.Tests.Mocks
{
    class MockProvider : IProvider
    {
        private readonly IStore _store;

        public MockProvider(IStore store, int? pollIntervalMs = null)
        {
            _store = store;
            PollIntervalMs = pollIntervalMs;
            HydraServer = _store.Name;
        }

        #region Implementation of IProvider

        public IStore GetStore(bool waitForInitialisation)
        {
            return _store;
        }

        public string HydraServer { get; private set; }

        public int? PollIntervalMs { get; private set; }

        public void ServerError(string server)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
