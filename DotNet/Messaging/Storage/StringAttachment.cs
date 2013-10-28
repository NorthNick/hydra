
namespace Shastra.Hydra.Messaging.Storage
{
    public class StringAttachment : Attachment
    {
        public string Data { get; private set; }

        public StringAttachment(string name, string data) : base(name)
        {
            Data = data;
        }
    }
}
