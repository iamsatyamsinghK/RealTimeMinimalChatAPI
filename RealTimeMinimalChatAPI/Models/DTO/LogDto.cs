namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class LogDto
    {
        public string IpAddress { get; set; }
        public string Username { get; set; }
        public string RequestPath { get; set; }
        public string RequestBody { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
