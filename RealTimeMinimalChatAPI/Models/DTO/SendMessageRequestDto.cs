namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class SendMessageRequestDto
    {
        public string ReceiverId { get; set; }
        public string Content { get; set; }
    }
}
