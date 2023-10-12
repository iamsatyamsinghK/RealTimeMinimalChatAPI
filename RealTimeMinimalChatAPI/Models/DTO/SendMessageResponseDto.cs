namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class SendMessageResponseDto
    {
        public int MessageId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
