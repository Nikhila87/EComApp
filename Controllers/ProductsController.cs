using EComAPI.Models;
using EComAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IO;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
        //List<int> a = new List<int>();
        //a.Add(1);
        //int b = Convert.ToInt16(a);
        //string s = "baby moon";
  
        

    }
  

    [HttpGet("debug-claims")]
    public IActionResult DebugClaims()
    {
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
        }
        foreach (var header in Request.Headers)
        {
            Console.WriteLine($"Header: {header.Key}, Value: {header.Value}");
        }
        var user = User.Identity?.Name;
        var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        return Ok($"Authenticated as {userName}");
        //var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        //return Ok(new { user, roles });
    }

    //[Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromHeader(Name = "Authorization")] string? authHeader = null)
    {
        return await _context.Products.ToListAsync();
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        return Ok(product);
    }




    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Roles ="Admin")]
    [HttpPost]
public async Task<ActionResult<Product>> CreateProduct(Product product, [FromHeader(Name = "Authorization")] string? authHeader = null)
    {


        try
        {
            List<string> savedImages = new List<string>(); 

            foreach (var image in product.ImageUrl.Split(',')) 
            {


                string sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "assets", image);
                string destinationFolder = Path.Combine(Directory.GetCurrentDirectory(), "images");
                string destinationPath = Path.Combine(destinationFolder, Path.GetFileName(sourcePath));

                if (!System.IO.File.Exists(sourcePath))
                {
                    return BadRequest($"Source file not found: {sourcePath}");
                }

                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);  //Access Denied here
                }

                System.IO.File.Copy(sourcePath, destinationPath, true);
                savedImages.Add("assets/"+Path.GetFileName(sourcePath));

            }

            //Save multiple images as a comma-separated string
            product.ImageUrl = string.Join(",", savedImages);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message} {ex.StackTrace}");
        }

    }
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [HttpDelete("products/{id}")]
    public IActionResult DeleteProduct(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound(new { message = "Products not found" });
        }

        _context.Products.Remove(product);
        _context.SaveChanges();
        return Ok(new { message = "Product deleted successfully" });
    }
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        product.Name = updatedProduct.Name;
        product.Description = updatedProduct.Description;
        product.Price = updatedProduct.Price;
        product.ImageUrl = updatedProduct.ImageUrl;
        _context.SaveChanges();
        return Ok(new { message = "Product updated successfully", product });
    }


    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var fileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine("wwwroot/uploads", fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
        return Ok(new { imageUrl });
    }


}
