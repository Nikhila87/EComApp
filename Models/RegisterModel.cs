using Microsoft.AspNetCore.Identity;

namespace EComAPI.Models
{
    public class RegisterModel
    {
        
        public string? UserName { get; set; }

        // The email address of the user
        public string? Email { get; set; }

        // The password the user wants to use
        public string Password { get; set; }

        // Confirm the password for validation
        public string? ConfirmPassword { get; set; }
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }


    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

}

