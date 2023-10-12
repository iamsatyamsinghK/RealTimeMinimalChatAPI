namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class SendMessageToNewChatRequestDto
    {
        public List<string> ReceiverIds { get; set; }
        public string Content { get; set; }
    }
}
