namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class SendMessageResponseCollectiveDto
    {
        public int MessageId { get; set; }
        public string SenderId { get; set; }
        public List<string> ReceiverIds { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
