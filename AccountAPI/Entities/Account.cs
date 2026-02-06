namespace AccountAPI.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string PasswordHash {  get; set; }
        public int RoleId { get; set; } = 2;
        public virtual Role Role { get; set; }
        public bool IsPasswordTemporary { get; set; }
        public int WrongPasswordCounter { get; set; }
        public DateTime? LastFailedLoginAttempt { get; set; }
    }
}
