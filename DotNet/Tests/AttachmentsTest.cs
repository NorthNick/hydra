using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shastra.Hydra.Messaging;
using Shastra.Hydra.Messaging.Attachments;
using Shastra.Hydra.Messaging.MessageFetchers;
using Shastra.Hydra.Messaging.MessageIds;
using Shastra.Hydra.Messaging.Storage;
using Shastra.Hydra.Tests.Mocks;

namespace Shastra.Hydra.Tests
{
    [TestClass]
    public class AttachmentsTest
    {
        private IProvider _provider;
        private IHydraService _service;
        private IStore _store;

        [TestInitialize]
        public void Initialize()
        {
            _store = new MockStore("AttachmentStore", "");
            _provider = new NearestServerProvider(new List<IStore> {_store});
            _service = new StdHydraService(_provider);
        }

        [TestMethod]
        public void TestSendReceiveAttachment()
        {
            const string topic = "TestSendReceiveAttachment";
            const string name = "TheName";
            const string sentData = "Some data";
            var sentAttachment = new StringAttachment(name, sentData);
            var sentId = _service.SendAsync(new HydraMessage { Topic = topic, Data = "foo", Attachments = new List<Attachment> { sentAttachment } }).Result;
            var fetcher = new HydraByTopicMessageFetcher(topic);

            var messages = fetcher.MessagesAfterIdUpToSeq(_store, MessageIdManager.Create(DateTime.UtcNow.AddHours(-1)), 100).ToList();
            Assert.AreEqual(1, messages.Count, "Should receive one message");

            var message = messages.First();
            Assert.IsNotNull(message.Attachments, "Message should have non-null Attachments");
            Assert.AreEqual(1, message.Attachments.Count(), "Message should have one attachment");

            var attachmentStub = message.Attachments.First();
            Assert.AreEqual(name, attachmentStub.Name, "Incorrect attachment name");
            Assert.AreEqual(sentId.ToDocId(), attachmentStub.MessageId.ToDocId(), "Incorrect attachment MessageId");

            var receivedContent = _store.GetAttachmentAsync(attachmentStub).Result;
            var receivedData = receivedContent.ReadAsStringAsync().Result;
            Assert.AreEqual(sentData, receivedData, "Incorrect attachment string data");
        }

        [TestMethod]
        public void TestMultipleAttachments()
        {
            const string topic = "TestMultipleAttachments";
            const int attachmentCount = 4;
            var attachments = Enumerable.Range(1, attachmentCount).Select(ii => new StringAttachment("TheName" + ii.ToString(), "Some data " + ii.ToString()));
            _service.SendAsync(new HydraMessage { Topic = topic, Data = "foo", Attachments = attachments }).Wait();
            var fetcher = new HydraByTopicMessageFetcher(topic);

            var messages = fetcher.MessagesAfterIdUpToSeq(_store, MessageIdManager.Create(DateTime.UtcNow.AddHours(-1)), 100).ToList();
            Assert.AreEqual(1, messages.Count, "Should receive one message");

            var message = messages.First();
            Assert.IsNotNull(message.Attachments, "Message should have non-null Attachments");
            Assert.AreEqual(attachmentCount, message.Attachments.Count(), string.Format("Message should have {0} attachments", attachmentCount));

            var dataSet = new HashSet<string>(message.Attachments.Select(att => _store.GetAttachmentAsync(att).Result.ReadAsStringAsync().Result));
            Assert.AreEqual(attachmentCount, dataSet.Count, "Attachments should all have different data");
        }
    }
}
