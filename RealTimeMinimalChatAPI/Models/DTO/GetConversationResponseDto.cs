namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class GetConversationResponseDto
    {
        public int MessageId { get; set; }
        public string SenderId { get; set; }
        public List<string> ReceiverIds { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public int ChatId { get; set; }
    }
}
