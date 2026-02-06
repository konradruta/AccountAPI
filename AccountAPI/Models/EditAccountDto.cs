using AccountAPI.Entities;

namespace AccountAPI.Models
{
    public class EditAccountDto
    {
        public string Name { get; set; }
        public int RoleId {  get; set; }
    }
}
