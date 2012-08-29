using System.Reactive;
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
    public class PollerTest : ReactiveTest
    {
        private TestScheduler _scheduler;
        private IConfigProvider _configProvider;
        private IHydraService _service;
        private IMessageFetcher<HydraMessage> _fetcher;
        private IStore _store;
        private DateTime _startDate;

        [TestInitialize]
        public void Initialize()
        {
            _scheduler = new TestScheduler();
            _store = new MockStore("PollerStore", "", _scheduler);
            _configProvider = new RoundRobinConfigProvider(new List<IStore> { _store }, 1000);
            _service = new HydraService(_configProvider);
            _fetcher = new HydraByTopicMessageFetcher("Test");
            // Any time after 1/1/1970 will do for startDate. CouchIds go wrong before that date as they are microseconds since 1/1/1970.
            _startDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        [TestMethod]
        public void TestSingleMessage()
        {
            // Send a message after 20 seconds
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(20)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Poller test", Data = "TestSingleMessage" }));
            // Dispose of the poller after 1 minute. (res disposes of its subscription to poller, but poller itself also has to be disposed of otherwise the test never terminates.)
            // Note that the scheduler unsubscribes before the disposal, otherwise it will get an OnCompleted event from the poller, and res.messages.Count will be one larger.
            Poller<HydraMessage> poller = null;
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddMinutes(1)), () => { poller.Dispose(); });
            var res = _scheduler.Start(() =>
                {
                    poller = new Poller<HydraMessage>(_configProvider, _fetcher, MessageIdManager.Create(_startDate.AddMinutes(-1)), 0, _scheduler);
                    return poller;
                },
                _startDate.Ticks, _startDate.Ticks, _startDate.AddSeconds(30).Ticks);
            Assert.AreEqual(1, res.Messages.Count(), "Should receive one message.");
        }

        [TestMethod]
        public void TestMultipleMessages()
        {
            // Send a few messages
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(20)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Poller test", Data = "TestMultipleMessages 1" }));
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(20)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Poller test", Data = "TestMultipleMessages 2" }));
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(23)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Poller test", Data = "TestMultipleMessages 3" }));

            Poller<HydraMessage> poller = null;
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddMinutes(1)), () => { poller.Dispose(); });
            var res = _scheduler.Start(() => {
                    poller = new Poller<HydraMessage>(_configProvider, _fetcher, MessageIdManager.Create(_startDate.AddMinutes(-1)), 0, _scheduler);
                    return poller;
                },
                _startDate.Ticks, _startDate.Ticks, _startDate.AddSeconds(30).Ticks);
            Assert.AreEqual(3, res.Messages.Count(), "Should receive three messages.");
        }

        [TestMethod]
        public void TestMissMessageSentBeforePolling()
        {
            // Send a message after 20 seconds
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(20)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Poller test", Data = "TestMissMessageSentBeforePolling" }));

            Poller<HydraMessage> poller = null;
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddMinutes(1)), () => { poller.Dispose(); });
            // Start polling 10 seconds after the message was sent, so we should not receive it.
            var pollerStartDate = _startDate.AddSeconds(30);
            var res = _scheduler.Start(() => {
                    poller = new Poller<HydraMessage>(_configProvider, _fetcher, MessageIdManager.Create(_scheduler.Now.UtcDateTime), 0, _scheduler);
                    return poller;
                },
                pollerStartDate.Ticks, pollerStartDate.Ticks, _startDate.AddSeconds(50).Ticks);
            Assert.AreEqual(0, res.Messages.Count(), "Should receive no message, as none were sent after polling started.");
        }

        [TestMethod]
        public void TestPollerSendsOnCompleted()
        {
            Poller<HydraMessage> poller = null;
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddMinutes(1)), () => { poller.Dispose(); });
            // Set disposal time after the poller has shut down, so we should get OnCompleted
            var res = _scheduler.Start(() => {
                poller = new Poller<HydraMessage>(_configProvider, _fetcher, MessageIdManager.Create(_scheduler.Now.UtcDateTime), 0, _scheduler);
                return poller;
            },
               _startDate.Ticks, _startDate.Ticks, _startDate.AddSeconds(100).Ticks);
            Assert.AreEqual(1, res.Messages.Count(), "Should receive one event.");
            Assert.AreEqual(NotificationKind.OnCompleted, res.Messages.First().Value.Kind, "The event should be OnCompleted");
        }

    }
}
