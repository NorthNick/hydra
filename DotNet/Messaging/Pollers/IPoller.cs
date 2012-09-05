using System;

namespace Bollywell.Hydra.Messaging.Pollers
{
    public interface IPoller<out TMessage> : IObservable<TMessage>, IDisposable where TMessage : TransportMessage
    {
        long BufferDelayMs { get; set; }
        long PollIntervalMs { get; set; }
        event Action<object, TMessage> MessageInQueue;
    }
}
