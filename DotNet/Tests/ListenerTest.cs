using System.Reactive;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.Listeners;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;
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
    public class ListenerTest : ReactiveTest
    {
        private TestScheduler _scheduler;
        private IConfigProvider _configProvider;
        private IHydraService _service;
        private IMessageFetcher<HydraMessage> _fetcher;
        private IStore _store;
        private DateTime _startDate;
        private Listener<HydraMessage> _listener;

        [TestInitialize]
        public void Initialize()
        {
            _scheduler = new TestScheduler();
            _store = new MockStore("ListenerStore", "", _scheduler);
            _configProvider = new RoundRobinConfigProvider(new List<IStore> { _store });
            _service = new HydraService(_configProvider);
            _fetcher = new HydraByTopicMessageFetcher("Test");
            // Any time after 1/1/1970 will do for startDate. CouchIds go wrong before that date as they are microseconds since 1/1/1970.
            _startDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // Dispose of the listener after 10 minutes. (res disposes of its subscription to poller, but poller itself also has to be disposed of otherwise the test never terminates.)
            // Note that the scheduler unsubscribes before the disposal, otherwise it will get an OnCompleted event from the listener, and res.messages.Count will be one larger.
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddMinutes(10)), () => _listener.Dispose());
        }

        [TestMethod]
        public void TestSingleMessage()
        {
            // Send a message after 20 seconds
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(20)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Listener test", Data = "TestSingleMessage" }));
            var res = _scheduler.Start(() =>
                {
                    _listener = new Listener<HydraMessage>(_configProvider, _fetcher, MessageIdManager.Create(_startDate.AddMinutes(-1)), null, _scheduler);
                    return _listener;
                },
                _startDate.Ticks, _startDate.Ticks, _startDate.AddSeconds(30).Ticks);
            Assert.AreEqual(1, res.Messages.Count(), "Should receive one message.");
        }

        [TestMethod]
        public void TestMultipleMessages()
        {
            // Send a few messages
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(20)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Listener test", Data = "TestMultipleMessages 1" }));
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(20)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Listener test", Data = "TestMultipleMessages 2" }));
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(23)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Listener test", Data = "TestMultipleMessages 3" }));

            var res = _scheduler.Start(() => {
                    _listener = new Listener<HydraMessage>(_configProvider, _fetcher, MessageIdManager.Create(_startDate.AddMinutes(-1)), null, _scheduler);
                    return _listener;
                },
                _startDate.Ticks, _startDate.Ticks, _startDate.AddSeconds(30).Ticks);
            Assert.AreEqual(3, res.Messages.Count(), "Should receive three messages.");
        }

        [TestMethod]
        public void TestMissMessageSentBeforePolling()
        {
            // Send a message after 20 seconds
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(20)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Listener test", Data = "TestMissMessageSentBeforePolling" }));

            // Start listening 10 seconds after the message was sent, so we should not receive it.
            var listenerStartDate = _startDate.AddSeconds(30);
            var res = _scheduler.Start(() => {
                    _listener = new Listener<HydraMessage>(_configProvider, _fetcher, MessageIdManager.Create(_scheduler.Now.UtcDateTime), null, _scheduler);
                    return _listener;
                },
                listenerStartDate.Ticks, listenerStartDate.Ticks, _startDate.AddSeconds(50).Ticks);
            Assert.AreEqual(0, res.Messages.Count(), "Should receive no message, as none were sent after polling started.");
        }

        [TestMethod]
        public void TestPollerSendsOnCompleted()
        {
            // Set disposal time after the poller has shut down, so we should get OnCompleted
            var res = _scheduler.Start(() => {
                    _listener = new Listener<HydraMessage>(_configProvider, _fetcher, MessageIdManager.Create(_scheduler.Now.UtcDateTime), null, _scheduler);
                    return _listener;
                },
               _startDate.Ticks, _startDate.Ticks, _startDate.AddMinutes(20).Ticks);
            Assert.AreEqual(1, res.Messages.Count(), "Should receive one event.");
            Assert.AreEqual(NotificationKind.OnCompleted, res.Messages.First().Value.Kind, "The event should be OnCompleted");
        }

        [TestMethod]
        public void TestMessagesInBufferWindowAreOrdered()
        {
            // Ordinary message after 20 seconds
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(20)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Listener test", Data = "second" }));
            // Send message after 22 seconds, predating the first by 1 second
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(22)), () => 
                _service.Send(new TestHydraMessage { Topic = "Test", Source = "Listener test", Data = "first", IdDate = _startDate.AddSeconds(19) }));

            // Set buffer window of 1500ms
            var res = _scheduler.Start(() => {
                    _listener = new Listener<HydraMessage>(_configProvider, _fetcher, MessageIdManager.Create(_scheduler.Now.UtcDateTime), new ListenerOptions {BufferDelayMs = 1500}, _scheduler);
                    return _listener;
                },
               _startDate.Ticks, _startDate.Ticks, _startDate.AddMinutes(9).Ticks);
            Assert.AreEqual(2, res.Messages.Count(), "Should receive two messages");
            Assert.AreEqual("firstsecond", res.Messages[0].Value.Value.Data + res.Messages[1].Value.Value.Data, "The second message sent should arrive before the first");
        }

        [TestMethod]
        public void TestMessagesOutsideBufferWindowAreNotOrdered()
        {
            // Ordinary message after 20 seconds
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(20)), () => _service.Send(new HydraMessage { Topic = "Test", Source = "Listener test", Data = "second" }));
            // Send message after 22 seconds, predating the first by 1 second
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddSeconds(22)), () =>
                _service.Send(new TestHydraMessage { Topic = "Test", Source = "Listener test", Data = "first", IdDate = _startDate.AddSeconds(19) }));

            // Set buffer window of 500ms
            var res = _scheduler.Start(() => {
                    _listener = new Listener<HydraMessage>(_configProvider, _fetcher, MessageIdManager.Create(_scheduler.Now.UtcDateTime), new ListenerOptions {BufferDelayMs = 500}, _scheduler);
                    return _listener;
                },
               _startDate.Ticks, _startDate.Ticks, _startDate.AddMinutes(9).Ticks);
            Assert.AreEqual(2, res.Messages.Count(), "Should receive two messages");
            Assert.AreEqual("secondfirst", res.Messages[0].Value.Value.Data + res.Messages[1].Value.Value.Data, "The second message sent should arrive after the first");
        }
    }
}
