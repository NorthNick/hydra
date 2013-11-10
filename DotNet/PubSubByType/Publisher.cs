using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shastra.Hydra.Messaging;
using Shastra.Hydra.Messaging.Attachments;
using Shastra.Hydra.Messaging.MessageIds;
using Shastra.Hydra.Messaging.Serializers;

namespace Shastra.Hydra.PubSubByType
{
    /// <summary>
    /// Class for publishing Hydra messages of a given type. To be used in conjunction with the Subscriber class.
    /// </summary>
    /// <typeparam name="TPub">The type of messages to publish.</typeparam>
    public class Publisher<TPub>
    {
        private readonly IHydraService _hydraService;
        private readonly string _topic;
        private readonly ISerializer<TPub> _serializer;

        /// <summary>
        /// An identifier for the sender of the messages.
        /// </summary>
        public string ThisParty { get; set; }

        /// <summary>
        /// Send messages to a specific destination.
        /// </summary>
        public string RemoteParty { get; set; }

        /// <summary>
        /// Create a new Publisher to send TPub messages.
        /// </summary>
        /// <param name="hydraService">The Hydra service to use for sending messages.</param>
        /// <param name="serializer">The serializer to use for TPub objects. Defaults to HydraDataContractSerializer.</param>
        /// <param name="topic">The topic for outgoing messages. Defaults to the ful name of TPub.</param>
        /// <remarks>The serializer must match the one used by the message subscriber.</remarks>
        public Publisher(IHydraService hydraService, ISerializer<TPub> serializer = null, string topic = null)
        {
            _hydraService = hydraService;
            _serializer = serializer ?? new HydraDataContractSerializer<TPub>();
            _topic = topic ?? typeof (TPub).FullName;
        }

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="remoteParty">Optional RemoteParty override.</param>
        /// <returns>The id of the message sent.</returns>
        [Obsolete("Send is deprecated. Please use SendAsync instead.")]
        public IMessageId Send(TPub message, string remoteParty = null)
        {
            return SendAsync(message, null, remoteParty).Result;
        }

        /// <summary>
        /// Send a message asynchronously.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="remoteParty">Optional RemoteParty override.</param>
        /// <returns>A Task returning the id of the message sent.</returns>
        public Task<IMessageId> SendAsync(TPub message, string remoteParty = null)
        {
            return SendAsync(message, null, remoteParty);
        }

        /// <summary>
        /// Send an augmented message.
        /// </summary>
        /// <param name="message">The augmented message to send.</param>
        /// <param name="remoteParty">Optional RemoteParty override.</param>
        /// <returns>The id of the message sent.</returns>
        [Obsolete("Send is deprecated. Please use SendAsync instead.")]
        public IMessageId Send(AugmentedMessage<TPub> message, string remoteParty = null)
        {
            return SendAsync(message.Message, message.Attachments, remoteParty).Result;
        }

        /// <summary>
        /// Send an augmented message asynchronously.
        /// </summary>
        /// <param name="message">The augmented message to send.</param>
        /// <param name="remoteParty">Optional RemoteParty override.</param>
        /// <returns>A Task returning the id of the message sent.</returns>
        public Task<IMessageId> SendAsync(AugmentedMessage<TPub> message, string remoteParty = null)
        {
            return SendAsync(message.Message, message.Attachments, remoteParty);
        }

        /// <summary>
        /// Send a message with attachments.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="attachments">Attachments to send with the message</param>
        /// <param name="remoteParty">Optional RemoteParty override.</param>
        /// <returns>The id of the message sent.</returns>
        [Obsolete("Send is deprecated. Please use SendAsync instead.")]
        public IMessageId Send(TPub message, IEnumerable<Attachment> attachments, string remoteParty = null)
        {
            return SendAsync(message, attachments, remoteParty).Result;
        }

        /// <summary>
        /// Send a message with attachments asynchronously.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="attachments">Attachments to send with the message</param>
        /// <param name="remoteParty">Optional RemoteParty override.</param>
        /// <returns>A Task returning the id of the message sent.</returns>
        public Task<IMessageId> SendAsync(TPub message, IEnumerable<Attachment> attachments, string remoteParty = null)
        {
            var hydraMessage = new HydraMessage { Source = ThisParty, Destination = remoteParty ?? RemoteParty, Topic = _topic, Data = _serializer.Serialize(message), Attachments = attachments };
            return _hydraService.SendAsync(hydraMessage);
        }
    }
}
