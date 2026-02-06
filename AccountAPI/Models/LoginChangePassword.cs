namespace AccountAPI.Models
{
    public class LoginChangePassword
    {
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string confirmNewPassword { get; set; }
    }
}
