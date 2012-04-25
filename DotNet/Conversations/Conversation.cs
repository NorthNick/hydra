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
        private long _nextSendSeq = 1, _nextRecvSeq = 1;

        public string Handle { get; private set; }
        public string RemoteParty { get; private set; }
        public string ThisParty { get; private set; }
        public string Topic { get; private set; }

        internal event Action<object> DoneEvent;

        internal void BaseInit(string thisParty, string remoteParty, string topic, string handle, ISerializer<TMessage> serializer)
        {
            ThisParty = thisParty;
            RemoteParty = remoteParty;
            Topic = topic;
            Handle = handle;
            _serializer = serializer;
        }

        internal void OnNext(long seq, TMessage message)
        {
            if (seq != _nextRecvSeq) {
                // Tell the client, but carry on and process the message.
                _subject.OnError(new Exception(string.Format("Sequence error. Expected {0}, received {1}", _nextRecvSeq, seq)));
            }
            _subject.OnNext(message);
            _nextRecvSeq = seq + 1;
        }

        /// <summary>
        /// Send a message to the other end of the conversation
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(TMessage message)
        {
            if (_done) return;

            var hydraMessage = new HydraMessage { Source = ThisParty, Destination = RemoteParty, Topic = Topic,
                                                  Handle = Handle, Seq = _nextSendSeq, Data = _serializer.Serialize(message) };
            hydraMessage.Send();
            _nextSendSeq++;
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
