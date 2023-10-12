namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class SendMessageRequestCollectiveDto
    {
       
        public List<string> ReceiverIds { get; set; }
        public string Content { get; set; }
    }
}
