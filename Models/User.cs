using Microsoft.AspNetCore.Identity;

namespace EComAPI.Models
{
    public class User:IdentityUser
    {

        public string? Role { get; set; }
        public string Password { get; set; }

    }
}

