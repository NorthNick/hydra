using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using Shastra.Hydra.Messaging;
using Shastra.Hydra.Messaging.Attachments;
using Shastra.Hydra.Messaging.MessageIds;
using Shastra.Hydra.Messaging.Serializers;

namespace Shastra.Hydra.Conversations
{
    public class Conversation<TMessage> : IObservable<Notification<AugmentedMessage<TMessage>>>, IDisposable
    {
        private ISerializer<TMessage> _serializer;
        private readonly Subject<Notification<AugmentedMessage<TMessage>>> _subject = new Subject<Notification<AugmentedMessage<TMessage>>>();
        private bool _done;
        private IHydraService _hydraService;

        public string Handle { get; private set; }
        public string RemoteParty { get; private set; }
        public string ThisParty { get; private set; }
        public string Topic { get; private set; }
        public long LastSendSeq { get; private set; }
        public long LastRecvSeq { get; private set; }
        public bool CheckSeq { get; set; }

        internal event Action<object> DoneEvent;

        internal void BaseInit(IHydraService hydraService, string thisParty, string remoteParty, string topic, string handle, ISerializer<TMessage> serializer)
        {
            _hydraService = hydraService;
            ThisParty = thisParty;
            RemoteParty = remoteParty;
            Topic = topic;
            Handle = handle;
            _serializer = serializer;
        }

        internal void OnNext(long seq, Notification<AugmentedMessage<TMessage>> message)
        {
            if (CheckSeq && seq != LastRecvSeq + 1) {
                // Tell the client, but carry on and process the message.
                _subject.OnError(new Exception(string.Format("Sequence error. Expected {0}, received {1}", LastRecvSeq + 1, seq)));
            }
            LastRecvSeq = seq;
            _subject.OnNext(message);
        }

        /// <summary>
        /// Send a message to the other end of the conversation, with optional attachments
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="attachments">Attachments to send with the message</param>
        /// <returns>The id of the message sent</returns>
        public IMessageId Send(TMessage message, IEnumerable<Attachment> attachments = null)
        {
            if (_done) return null;

            var hydraMessage = new HydraMessage { Source = ThisParty, Destination = RemoteParty, Topic = Topic, Attachments = attachments,
                                                  Handle = Handle, Seq = LastSendSeq + 1, Data = _serializer.Serialize(message) };
            var res = _hydraService.Send(hydraMessage);
            // Increment LastSendSeq after sending in case the Send fails.
            LastSendSeq++;
            return res;
        }

        /// <summary>
        /// Send an augmented message to the other end of the conversation
        /// </summary>
        /// <param name="message">The augmented message to send.</param>
        /// <returns>The id of the message sent.</returns>
        public IMessageId Send(AugmentedMessage<TMessage> message)
        {
            return Send(message.Message, message.Attachments);
        }

        #region Implementation of IObservable<out Notification<AugmentedMessage<TMessage>>>

        public IDisposable Subscribe(IObserver<Notification<AugmentedMessage<TMessage>>> observer)
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
                _done = true;
                _subject.Dispose();
                if (DoneEvent != null) DoneEvent(this);
            }
            // free native resources if there are any.
        }

        #endregion

    }
}
