namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class GetConversationRequestDto
    {
        public int ChatId { get; set; }
        public DateTime? Before { get; set; }
        public int Count { get; set; }
        public string Sort { get; set; }
    }
}
