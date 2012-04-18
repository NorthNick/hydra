using System;
using System.Reactive.Subjects;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Serializers;

namespace Bollywell.Hydra.Conversations
{
    public class Conversation<TMessage> : IObservable<TMessage>
    {
        private ISerializer<TMessage> _serializer;
        private readonly Subject<TMessage> _subject = new Subject<TMessage>();
        private bool _done;

        public string Handle { get; private set; }
        public string RemoteParty { get; private set; }
        public string ThisParty { get; private set; }
        public string Topic { get; private set; }

        internal event Action<object> DoneEvent;

        internal void OnNext(TMessage message) { _subject.OnNext(message); }

        internal void BaseInit(string thisParty, string remoteParty, string topic, string handle, ISerializer<TMessage> serializer)
        {
            ThisParty = thisParty;
            RemoteParty = remoteParty;
            Topic = topic;
            Handle = handle;
            _serializer = serializer;
        }

        /// <summary>
        /// Send a message to the other end of the conversation
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(TMessage message)
        {
            if (_done) return;

            var hydraMessage = new HydraMessage { Source = ThisParty, Destination = RemoteParty, Topic = Topic,
                                                  Handle = Handle, Data = _serializer.Serialize(message) };
            hydraMessage.Send();
        }

        /// <summary>
        /// Call this when the conversation is over. Notifies the switchboard that it should ignore future messages with this handle.
        /// </summary>
        public void Done()
        {
            _done = true;
            if (DoneEvent == null) return;
            DoneEvent(this);
        }

        #region Implementation of IObservable<out TMessage>

        public IDisposable Subscribe(IObserver<TMessage> observer)
        {
            return _subject.Subscribe(observer);
        }

        #endregion
    }
}
