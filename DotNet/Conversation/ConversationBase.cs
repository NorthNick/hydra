using System;
using Bollywell.Hydra.Messaging;
using Bollywell.Hydra.Messaging.Serializers;

namespace Bollywell.Hydra.Conversation
{
    public abstract class ConversationBase<TMessage>
    {
        private ISerializer<TMessage> _serializer;

        public string Handle { get; private set; }
        public string RemoteParty { get; private set; }
        public string ThisParty { get; private set; }
        public string Topic { get; private set; }

        /// <summary>
        /// Process a message.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <remarks>Messages are all processed on the same thread so, if processing takes any time, do it asynchronously and
        /// return from this call immediately.</remarks>
        public abstract void OnMessage(TMessage message);

        internal event Action<object> DoneEvent;

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
        protected void Send(TMessage message)
        {
            var hydraMessage = new HydraMessage { Source = ThisParty, Destination = RemoteParty, Topic = Topic,
                                                  Handle = Handle, Data = _serializer.Serialize(message) };
            hydraMessage.Send();
        }

        /// <summary>
        /// Call this when the conversation is over. Notifies the switchboard that it should ignore future messages with this handle.
        /// </summary>
        protected void Done()
        {
            if (DoneEvent == null) return;
            DoneEvent(this);
        }
    }
}
