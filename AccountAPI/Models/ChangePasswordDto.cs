namespace AccountAPI.Models
{
    public class ChangePasswordDto
    {
        public string Email { get; set; }
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string confirmNewPassword {  get; set; }
    }
}
