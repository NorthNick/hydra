using System;
using System.Collections.Generic;
using Bollywell.Hydra.Messaging.MessageIds;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bollywell.Hydra.Tests
{
    [TestClass]
    public class MessageIdManagerTest
    {

        [TestMethod]
        public void TestIdentifiesValidMessageIds()
        {
            var validIds = new List<string> {
                "abcdef12345678",           // no suffix
                "def123abcd1234990",        // numeric suffix
                "12345678900987helloworld"  // char suffix
            };
            foreach (var validId in validIds) {
                Assert.IsTrue(MessageIdManager.IsMessageId(validId), "MessageIdManager.IsMessageId should succeed on {0}", validId);
            }
        }

        [TestMethod]
        public void TestIdentifiesInvalidMessageIds()
        {
            var invalidIds = new List<string> {
                "",                 // No characters
                "1234",             // Too short
                "Abcdef12345678",   // Upper case characters
                "abcdef1234567j",   // Non-hex characters
                null                // null
            };
            foreach (var invalidId in invalidIds) {
                Assert.IsFalse(MessageIdManager.IsMessageId(invalidId), "Message.IsMessageId should fail on {0}", invalidId ?? "null");
            }
        }

        [TestMethod]
        public void TestPreservesMillisecondDateAccuracy()
        {
            var idDate = DateTime.Parse("2012-08-13T15:00:50.156Z");
            var messageId = MessageIdManager.Create(idDate);
            Assert.AreEqual(idDate, messageId.ToDateTime(), "MessageId.ToDateTime for {0} yields {1}", idDate.ToString("o"), messageId.ToDateTime().ToString("o"));
        }

        [TestMethod]
        public void TestPreservesDocId()
        {
            const string docId = "56f3aa0b78095dabracadabra";
            var messageId = MessageIdManager.Create(docId);
            Assert.AreEqual(docId, messageId.ToDocId(), "MessageId.ToDocId for {0} yields {1}", docId, messageId.ToDocId());
        }

    }
}
