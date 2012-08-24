using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;
using Bollywell.Hydra.Messaging.Pollers;
using Bollywell.Hydra.Tests.Mocks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

namespace Bollywell.Hydra.Tests
{
    // See http://blogs.msdn.com/b/rxteam/archive/2012/06/14/testing-rx-queries-using-virtual-time-scheduling.aspx
    [TestClass]
    public class TestPoller : ReactiveTest
    {

        [TestMethod]
        public void TestSingleMessage()
        {
            var scheduler = new TestScheduler();
            var store = new MockStore("PollerStore", "", scheduler);
            var configProvider = new RoundRobinConfigProvider(new List<IStore> { store }, 1000);
            var service = new HydraService(configProvider);
            var fetcher = new HydraByTopicMessageFetcher("Test");
            
            // Any time after 1/1/1970 will do for startDate. CouchIds go wrong before that date as they are microseconds since 1/1/1970.
            var startDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var startTicks = startDate.Ticks;
            // Send a message after 20 seconds
            scheduler.Schedule(new DateTimeOffset(startDate.AddSeconds(20)), () => service.Send(new HydraMessage { Topic = "Test", Source = "Poller test", Data = "Test data" }));
            // Dispose of the poller after 1 minute. (res disposes of its subscription to poller, but poller itself also has to be disposed of otherwise the test never terminates.)
            Poller<HydraMessage> poller = null;
            scheduler.Schedule(new DateTimeOffset(startDate.AddMinutes(1)), () => { poller.Dispose(); });
            var res = scheduler.Start(() =>
                {
                    poller = new Poller<HydraMessage>(configProvider, fetcher, MessageIdManager.Create(startDate.AddMinutes(-1)), 0, scheduler);
                    return poller;
                },
                startTicks, startTicks, startDate.AddSeconds(30).Ticks);
            Assert.AreEqual(1, res.Messages.Count());
        }

    }
}
