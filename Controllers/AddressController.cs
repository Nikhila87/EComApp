using EComAPI.Data;
using EComAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EComAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public AddressController(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("address")]
     
        public async Task<IActionResult> AddAddress(AddressDto dto)
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var user = await _userManager.FindByNameAsync(username);
            if (username== null) return Unauthorized();
           
            var address = new Address
            {
                FullName = dto.FullName,
                Street = dto.Street,
                City = dto.City,
                ZipCode = dto.ZipCode,
                Country = dto.Country,
                UserId= user.Id

            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Address added successfully." });
        }

        [HttpGet("get/{username}")]
        
        public async Task<IActionResult> GetMyAddresses(string username)
        {

            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return Unauthorized();

            var addresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();
            return Ok(addresses);
            //return Ok(new {addresses });
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("defaultaddr/{id}")]
        //[AllowAnonymous]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
  

            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return Unauthorized();

            // Unset previous default addresses
            var addresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            foreach (var addr in addresses)
                addr.IsDefault = false;

            var defaultAddress = addresses.FirstOrDefault(a => a.Id == id);
            if (defaultAddress != null)
                defaultAddress.IsDefault = true;

            await _context.SaveChangesAsync();

            return Ok(defaultAddress);
        }
        [HttpGet("default/{username}")]
        public async Task<ActionResult<Address>> GetDefaultAddress(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            var defaultAddress = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.IsDefault);

            if (defaultAddress == null)
                return NotFound();

            return Ok(defaultAddress);
        }
    }

}
