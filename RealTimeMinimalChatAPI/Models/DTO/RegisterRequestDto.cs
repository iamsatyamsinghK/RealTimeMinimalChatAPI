using System.ComponentModel.DataAnnotations;

namespace RealTimeMinimalChatAPI.Models.DTO
{
    public class RegisterRequestDto
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        //[StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least 6 characters long.")]
        public string Password { get; set; }
    }
}
