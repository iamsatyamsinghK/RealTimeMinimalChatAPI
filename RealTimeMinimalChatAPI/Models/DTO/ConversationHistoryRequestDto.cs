namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class ConversationHistoryRequestDto
    {
        public string UserId { get; set; }
        public DateTime? Before { get; set; }
        public int Count { get; set; }
        public string Sort { get; set; }
    }
}
