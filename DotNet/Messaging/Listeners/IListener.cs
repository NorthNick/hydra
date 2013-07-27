using System;

namespace Shastra.Hydra.Messaging.Listeners
{
    public interface IListener<out TMessage> : IObservable<TMessage>, IDisposable where TMessage : TransportMessage
    {
        long BufferDelayMs { get; set; }
        long PollIntervalMs { get; set; }
        event Action<object, TMessage> MessageInQueue;
    }
}
