using AccountAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace AccountAPI.Models
{
    public class CreateAccountDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
    }
}
