using System.ComponentModel;

namespace AccountAPI.Models
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        /*[DefaultValue(false)]
        public bool AccteptSend { get; set; }*/
    }
}
