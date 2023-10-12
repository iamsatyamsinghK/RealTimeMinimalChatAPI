using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RealTimeMinimalChatAPI.Models.Domain
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Creator")]
        public string CreatorId { get; set; }
        public ApplicationUser Creator { get; set; }

        public ICollection<ChatUser> ChatUsers { get; set; }
        public ICollection<Message> Messages { get; set; }

        public Chat()
        {
            Messages = new List<Message>();
        }
    }

}
