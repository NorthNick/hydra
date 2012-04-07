using System;

namespace Bollywell.Hydra.Messaging.Pollers
{
    public interface IPoller<out TMessage> : IObservable<TMessage>, IDisposable where TMessage : TransportMessage
    {
        event Action<object, TMessage> MessageInQueue;
    }
}
