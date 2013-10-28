using System.Collections.Generic;
namespace Shastra.Hydra.Messaging.Storage
{
    public interface IViewOptions
    {
        bool IncludeDocs { get; }
        IKeyOptions StartKey { get; }
        IKeyOptions EndKey { get; }
        IEnumerable<IKeyOptions> Keys { get; }
    }

    public interface IKeyOptions {}

}
