using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RealTimeMinimalChatAPI.Models.Domain
{
    public class MessageReceiver
    {
        [Key]

        [ForeignKey("Message")]
        public int MessageId { get; set; }

        // Navigation property for the associated message

        public Message Message { get; set; }

        [ForeignKey("Receiver")]
        public string ReceiverId { get; set; }

        // Navigation property for the message receiver (user)

        public ApplicationUser Receiver { get; set; }
    }
}
