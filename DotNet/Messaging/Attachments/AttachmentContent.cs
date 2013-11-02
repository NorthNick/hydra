using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shastra.Hydra.Messaging.Attachments
{
    public class AttachmentContent
    {
        private readonly HttpContent _content;

        internal AttachmentContent(HttpContent content)
        {
            _content = content;
        }

        public Task<byte[]> ReadAsByteArrayAsync() { return _content.ReadAsByteArrayAsync(); }

        public Task<Stream> ReadAsStreamAsync() { return _content.ReadAsStreamAsync(); }

        public Task<string> ReadAsStringAsync() { return _content.ReadAsStringAsync(); }
    }
}
