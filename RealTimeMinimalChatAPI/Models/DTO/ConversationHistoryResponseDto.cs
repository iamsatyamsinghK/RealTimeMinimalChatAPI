﻿namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class ConversationHistoryResponseDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
