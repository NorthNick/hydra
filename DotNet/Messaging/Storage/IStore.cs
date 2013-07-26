using System.Collections.Generic;
using Bollywell.Hydra.Messaging.MessageIds;
using LoveSeat.Interfaces;
using Newtonsoft.Json.Linq;

namespace Bollywell.Hydra.Messaging.Storage
{
    public interface IStore
    {
        string Name { get; }
        IEnumerable<IMessageId> GetChanges(IMessageId startId, long sinceSeq, out long lastSeq);
        long GetLastSeq();
        // SaveDoc would be better taking a JToken, but that's not easily doable with LoveSeat
        IMessageId SaveDoc(string json);
        IEnumerable<JToken> GetDocs(string viewName, IViewOptions options);
        ServerDistanceInfo MeasureDistance();
    }
}
