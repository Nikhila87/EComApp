using Microsoft.AspNetCore.Identity;

namespace EComAPI.Models
{
    public class User:IdentityUser
    {
        //public string UserName { get; set; }  // or 'Username'
        //public string Email { get; set; }
        //public override DateTimeOffset? LockoutEnd { get; set; }
        public string? Role { get; set; }
        //public string Password { get; set; }

        //public string name { get; set; }
        public ICollection<Address> Addresses { get; set; }
    }
}

