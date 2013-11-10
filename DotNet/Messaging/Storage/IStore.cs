using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Shastra.Hydra.Messaging.Attachments;
using Shastra.Hydra.Messaging.MessageIds;

namespace Shastra.Hydra.Messaging.Storage
{
    public interface IStore
    {
        string Name { get; }
        IEnumerable<IMessageId> GetChanges(IMessageId startId, long sinceSeq, out long lastSeq);
        long GetLastSeq();
        Task<IMessageId> SaveDocAsync(JObject json, IEnumerable<Attachment> attachments = null);
        IEnumerable<JToken> GetDocs(string viewName, IViewOptions options);
        Task<AttachmentContent> GetAttachmentAsync(Attachment attachment);
        ServerDistanceInfo MeasureDistance();
    }
}
