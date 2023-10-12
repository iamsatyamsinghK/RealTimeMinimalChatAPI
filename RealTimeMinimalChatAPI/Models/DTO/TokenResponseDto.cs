using Microsoft.AspNetCore.Identity;

namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class TokenResponseDto
    {
        public string? Token { get; set; }
        public IdentityUser? User { get; set; }
        public bool? IsSuccessful { get; set; }
        
        public string? Name { get; set; }
    }
}
