using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;
using System;

namespace Bollywell.Hydra.Tests.Mocks
{
    class MockConfigProvider : IConfigProvider
    {
        private readonly IStore _store;

        public MockConfigProvider(IStore store, int? pollIntervalMs = null)
        {
            _store = store;
            PollIntervalMs = pollIntervalMs;
            HydraServer = _store.Name;
        }

        #region Implementation of IConfigProvider

        public IStore GetStore()
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
