using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EComAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;

    public AuthController(IConfiguration config, UserManager<User> userManager)
    {
        _config = config;
        _userManager = userManager;
    }
    


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        try
        {
      
        var existingUser = await _userManager.FindByNameAsync(model.UserName);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Username is already taken." });
        }
       
            // Create new user
            var user = new User { UserName = model.UserName, Email = model.Email, Password = model.Password };
            var result = await _userManager.CreateAsync(user, model.Password);
            // Get user roles from database
            
            if (result.Succeeded)
            {
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
            new Claim(ClaimTypes.Email, "nikhilajathanna@gmail.com"?? "UnknownEmail"),
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
}

public class LoginModel
{
    public string ?Username { get; set; }
    public required string Password { get; set; } 
}
