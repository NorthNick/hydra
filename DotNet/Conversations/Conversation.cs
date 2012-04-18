using System;
using System.Reactive.Subjects;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Serializers;

namespace Bollywell.Hydra.Conversations
{
    public class Conversation<TMessage> : IObservable<TMessage>, IDisposable
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

        #region Implementation of IObservable<out TMessage>

        public IDisposable Subscribe(IObserver<TMessage> observer)
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
