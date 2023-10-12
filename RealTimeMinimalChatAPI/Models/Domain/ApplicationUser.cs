using Microsoft.AspNetCore.Identity;

namespace RealTimeMinimalChatAPI.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<ChatUser> ChatUsers { get; set; }
        public ICollection<MessageReceiver> Receivers { get; set; }

    }
}
