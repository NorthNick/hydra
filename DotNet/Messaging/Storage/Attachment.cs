
namespace Shastra.Hydra.Messaging.Storage
{
    public abstract class Attachment
    {
        public string Name { get; private set; }

        protected Attachment(string name)
        {
            Name = name;
        }
    }
}
