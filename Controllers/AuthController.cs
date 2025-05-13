using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EComAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EComAPI.NewFolder;
using System.Net;
using System.Web;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;

    public AuthController(IConfiguration config, UserManager<User> userManager,IEmailService emailService)
    {
        _config = config;
        _userManager = userManager;
        _emailService = emailService;
    }
    


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        try
        {
            Console.WriteLine("🔥 Debugging: New changes are working! 🔥");
        // Check if the user already exists
        var existingUser = await _userManager.FindByNameAsync(model.UserName);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Username is already taken." });
        }
       
            // Create new user
            var user = new User { UserName = model.UserName, Email = model.Email,Role="User"};
            var result = await _userManager.CreateAsync(user, model.Password); //password hashing
            // Get user roles from database
            
            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Encode token
                var encodedToken = HttpUtility.UrlEncode(token);

                // Generate link
                var confirmationLink = $"{_config["FrontendBaseUrl"]}/#/confirm-email?email={user.Email}&token={encodedToken}";

                // Send email
                await _emailService.SendEmailAsync(user.Email, "Confirm your email", $"Click here to confirm: {confirmationLink}");
                return Ok(new { message = "User registered successfully." });
            }
            else
            {
                Console.WriteLine("Registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors);
            }
            //return Ok("hello");
            //return BadRequest(new { error = "Registration failed." });
        }
        catch (Exception ex)
        {
            // Log the exception 
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine($"UserName: {model.UserName}, Email: {model.Email}, PhoneNumber: {model.Password}");
            // Return error message to the user
            return StatusCode(500, "An error occurred while processing your request.");
        }

    }


    



    [HttpPost("login")]
    public async Task<IActionResult>  Login([FromBody] LoginModel model)
    {
       


        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            return BadRequest("User not found.");
        }
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return Unauthorized(new { message="Please confirm your email before logging in." });

        }
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isPasswordValid)
        {
            return Unauthorized("Invalid username or password.");
        }

        //var user = new User
        //{
        //    UserName = model.Username,
        //    Email = "admin@example.com",   // Example Email (could come from database in a real case)
        //    //Role =model.Roles,        // Example Role (could come from database in a real case)
        //    Password = model.Password      // Example Password (not recommended to store plain passwords like this)
        //};
        var roles = await _userManager.GetRolesAsync(user);
        Console.WriteLine($"Roles for {user.UserName}: {string.Join(", ", roles)}");
        Console.WriteLine($"User Found: {user.UserName} - {user.Id}");
        //if (user.Role == "Admin")
        //{
        var token = GenerateJwtToken(user,roles);
            return Ok(new { token });
        //}
        //return Unauthorized();
    }

    private string GenerateJwtToken(EComAPI.Models.User user, IList<string> roles)
    {
        var jwtKey = _config["JwtSettings:Key"];
        Console.WriteLine($"JWT Key in AuthController: {jwtKey}");
        //byte[] secret = Encoding.UTF8.GetBytes(jwtKey ?? string.Empty);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName?? "UnknownUser"),
            new Claim(ClaimTypes.Email, user.Email?? "UnknownEmail"),
             //new Claim(ClaimTypes.Role, "Admin")
        };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
      


        var token = new JwtSecurityToken(
            _config["JwtSettings:Issuer"],
            _config["JwtSettings:Audience"],
            claims:claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds
        );
        //Console.WriteLine($"Generated Token-14/03: {tokenString}");
        Console.WriteLine($"JWT Key-14/03: {_config["JwtSettings:Key"]}");

        //return Ok(new { token = tokenString });
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        //var user = await _userManager.Users
        // .Where(u => u.Email.ToLower() == model.Email.ToLower())
        // .FirstOrDefaultAsync();
        if (user == null) return Ok(); // Don't reveal user existence
        var frontendUrl = _config["FrontendBaseUrl"];
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
        //var resetLink = Url.Action("ResetPassword",
        //    new { token, email = user.Email },
        //    protocol: HttpContext.Request.Scheme);
        var subject = "Reset Your Password";
        var body = $"<p>Click below to reset your password:</p><a href='{resetLink}'>Reset Password</a>";

        await _emailService.SendEmailAsync(model.Email, subject, body);

        // TODO: Send resetLink via email
        Console.WriteLine(resetLink); // For now, just log it or send via frontend
        return Ok("Password reset email sent.");
        //return Ok("Password reset link has been sent."+resetLink);
        //return Ok(new { resetLink });
    }


    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromQuery] string token, [FromQuery] string email, [FromBody] ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return BadRequest("Invalid request");
        var decodedToken = Uri.UnescapeDataString(model.Token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

                if (result.Succeeded)
        {
            return Ok(new { messaage = "Password has been reset." });
        }

        return BadRequest(result.Errors);
    }
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string email, string token)
    {
        //var user = await _userManager.FindByEmailAsync(email);
        var user = await _userManager.Users
       .FirstOrDefaultAsync(u => u.Email == email);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            return BadRequest("Invalid email confirmation request.");
        if (user == null) return BadRequest("Invalid email.");
        string decodedToken = HttpUtility.UrlDecode(token);
        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded) return BadRequest("Email confirmation failed.");

        return Ok(new { message = "Email confirmed successfully." });
    }

}

public class LoginModel
{
    public string ?Username { get; set; }
    public required string Password { get; set; } 
}
