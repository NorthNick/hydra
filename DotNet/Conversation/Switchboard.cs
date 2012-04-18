﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reactive.Subjects;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Config;
using Bollywell.Hydra.Messaging.MessageFetchers;
using Bollywell.Hydra.Messaging.Pollers;
using Bollywell.Hydra.Messaging.Serializers;

namespace Bollywell.Hydra.Conversation
{
    public class Switchboard<TConversation, TMessage> : IObservable<TConversation>, IDisposable where TConversation : ConversationBase<TMessage>, new()
    {
        // Maps handles to their conversations
        private readonly Dictionary<string, TConversation> _conversations = new Dictionary<string, TConversation>();
        private readonly HashSet<string> _deadConversations = new HashSet<string>();
        private readonly Poller<HydraMessage> _poller;
        private readonly Subject<TConversation> _subject = new Subject<TConversation>();
        private readonly ISerializer<TMessage> _serializer;
        private readonly string _thisParty;
        private readonly string _topic;

        /// <summary>
        /// Create a new Switchboard to listen for incoming conversations and initiate outgoing ones.
        /// </summary>
        /// <param name="thisParty">Name of this end of the conversation. This will be the RemoteParty for anyone initiating a conversation with this app.</param>
        /// <param name="topic">Topic of the conversation.</param>
        /// <param name="serializer">Optional serialiser for messages. Defaults to DataContractSerializer.</param>
        public Switchboard(string thisParty, string topic = null, ISerializer<TMessage> serializer = null)
        {
            _thisParty = thisParty;
            _topic = topic ?? typeof (TMessage).FullName;
            _serializer = serializer ?? new HydraDataContractSerializer<TMessage>();

            string pollSetting = ConfigurationManager.AppSettings["PollIntervalMs"];
            int? pollIntervalMs = pollSetting == null ? (int?) null : int.Parse(pollSetting);
            Services.DbConfigProvider = new AppDbConfigProvider(ConfigurationManager.AppSettings["HydraServer"], ConfigurationManager.AppSettings["Database"], pollIntervalMs);

            _poller = new Poller<HydraMessage>(new HydraByTopicByDestinationMessageFetcher(_topic, thisParty));
            _poller.Subscribe(OnMessage);
        }

        /// <summary>
        /// Initiate a new conversation.
        /// </summary>
        /// <param name="remoteParty">The other party in the conversation.</param>
        /// <returns>The conversation</returns>
        public TConversation NewConversation(string remoteParty)
        {
            string handle = Guid.NewGuid().ToString("N");
            return CreateNewConversation(remoteParty, handle);
        }

        private void OnMessage(HydraMessage message)
        {
            string handle = message.Handle;
            if (_deadConversations.Contains(handle)) return;

            if (!_conversations.ContainsKey(handle)) {
                CreateNewConversation(message.Source, handle);
            }
            _conversations[handle].OnMessage(_serializer.Deserialize(message.Data));
        }

        private TConversation CreateNewConversation(string remoteParty, string handle)
        {
            var conversation = new TConversation();
            conversation.DoneEvent += ConversationDoneEvent;
            conversation.BaseInit(_thisParty, remoteParty, _topic, handle, _serializer);
            _conversations[handle] = conversation;
            _subject.OnNext(conversation);
            return conversation;
        }

        private void ConversationDoneEvent(object obj)
        {
            var conversation = (TConversation) obj;
            conversation.DoneEvent -= ConversationDoneEvent;
            _conversations.Remove(conversation.Handle);
            _deadConversations.Add(conversation.Handle);
        }

        #region Implementation of IObservable<out TConversation>

        public IDisposable Subscribe(IObserver<TConversation> observer)
        {
            return _subject.Subscribe(observer);
        }

        #endregion

        #region Implementation of IDisposable

        // See http://msdn.microsoft.com/en-us/library/ms244737.aspx

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                // free managed resources
                _poller.Dispose();
                _subject.Dispose();
            }
            // free native resources if there are any.
        }

        #endregion

    }
}
