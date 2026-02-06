namespace AccountAPI
{
    public class AuthenticationSettings
    {
        public string JwtKey {  get; set; }
        public string JwtIssuer { get; set; }
        public int JwtExpiresDays { get; set; }
    }
}
