namespace Bollywell.Hydra.ConversationExampleDto
{
    public class ConversationDto
    {
        public const string ConversationTopic = "AppendConversation";

        public MessageTypes MessageType { get; set; }
        public string Data { get; set; }
    }
}
