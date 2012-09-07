using System;
using System.Collections.Generic;
using System.Linq;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.MessageIds;
using Bollywell.Hydra.Messaging.Storage;
using Bollywell.Hydra.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bollywell.Hydra.Tests
{
    [TestClass]
    public class MessageFetcherTest
    {
        private const string MessageSource = "MessageFetcherTest";
        private const string Alternating1 = "Alternating1";
        private const string Alternating2 = "Alternating2";
        private const int AlternatingMessageCount = 5;
        private IStore _singleMessageStore;
        private IStore _alternatingStore;

        [TestInitialize]
        public void Initialize()
        {
            // Set up stores
            _singleMessageStore = new MockStore("SingleMessageStore", "");
            var configProvider = new RoundRobinProvider(new List<IStore> { _singleMessageStore });
            var service = new HydraService(configProvider);
            service.Send(new HydraMessage { Topic = "Test", Source = MessageSource, Data = "Test data" });

            _alternatingStore = new MockStore("AlternatingStore", "");
            configProvider = new RoundRobinProvider(new List<IStore> { _alternatingStore });
            service = new HydraService(configProvider);
            for (int ii=0; ii < AlternatingMessageCount; ii++) {
                service.Send(new HydraMessage { Topic = Alternating1, Source = MessageSource, Data = string.Format("{0} message {1}", Alternating1, ii) });
                service.Send(new HydraMessage { Topic = Alternating2, Source = MessageSource, Data = string.Format("{0} message {1}", Alternating2, ii) });
            }
        }

        [TestMethod]
        public void TestSingleMessageByTopicFetcher()
        {
            Assert.AreEqual(_singleMessageStore.GetLastSeq(), 0, "LastSeq should be zero after sending one message");

            var fetcher = new HydraByTopicMessageFetcher("Test");
            var messages = fetcher.MessagesAfterIdUpToSeq(_singleMessageStore, MessageIdManager.Create(DateTime.UtcNow.AddHours(-1)), 1);
            Assert.AreEqual(1, messages.Count(), "Should receive one message for topic Test");

            fetcher = new HydraByTopicMessageFetcher("Other");
            messages = fetcher.MessagesAfterIdUpToSeq(_singleMessageStore, MessageIdManager.Create(DateTime.UtcNow.AddHours(-1)), 1);
            Assert.AreEqual(0, messages.Count(), "Should receive no messages for topic Other");
        }

        [TestMethod]
        public void TestAlternatingByTopicFetcher()
        {
            Assert.AreEqual(_alternatingStore.GetLastSeq(), AlternatingMessageCount * 2 - 1, "LastSeq should be nine after sending ten messages");

            var fetcher = new HydraByTopicMessageFetcher(Alternating1);
            // All Alternating1 messages
            var messages = fetcher.MessagesAfterIdUpToSeq(_alternatingStore, MessageIdManager.Create(DateTime.UtcNow.AddHours(-1)), AlternatingMessageCount * 2);
            Assert.AreEqual(AlternatingMessageCount, messages.Count(), string.Format("Should receive {0} messages for topic {1}", AlternatingMessageCount, Alternating1));

            // Initial messages up to a SeqId
            messages = fetcher.MessagesAfterIdUpToSeq(_alternatingStore, MessageIdManager.Create(DateTime.UtcNow.AddHours(-1)), 4);
            Assert.AreEqual(3, messages.Count(), string.Format("Should receive three messages for topic {0} up to SeqId 4", Alternating1));

            // All messages after the first
            var firstMessageId = messages.First().MessageId;
            messages = fetcher.MessagesAfterIdUpToSeq(_alternatingStore, firstMessageId, AlternatingMessageCount * 2);
            Assert.AreEqual(AlternatingMessageCount - 1, messages.Count(), string.Format("Should receive {0} messages for topic {1} after first MessageId", AlternatingMessageCount - 1, Alternating1));

            // Messages after the first, up to a SeqId
            messages = fetcher.MessagesAfterIdUpToSeq(_alternatingStore, firstMessageId, 4);
            Assert.AreEqual(2, messages.Count(), string.Format("Should receive two messages for topic {0} after first MessageId, up to SeqId 4", Alternating1));

            fetcher = new HydraByTopicMessageFetcher("Other");
            messages = fetcher.MessagesAfterIdUpToSeq(_alternatingStore, MessageIdManager.Create(DateTime.UtcNow.AddHours(-1)), 1);
            Assert.AreEqual(0, messages.Count(), "Should receive no messages for topic Other");
        }
    }
}
