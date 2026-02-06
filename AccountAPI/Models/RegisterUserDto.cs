using System.ComponentModel.DataAnnotations;

namespace AccountAPI.Models
{
    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Name {  get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
