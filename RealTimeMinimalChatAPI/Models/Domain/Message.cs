using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace RealTimeMinimalChatAPI.Models.Domain
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Sender")]    
        [Required]
        public string SenderId { get; set; }


       

        [Required]
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation property for the sender
        public ApplicationUser Sender { get; set; }

        // Navigation property for the message receivers
        public ICollection<MessageReceiver> Receivers { get; set; }
        public Chat? Chat { get; set; }

    }
}
