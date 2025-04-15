﻿using EComAPI.Models;
using EComAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IO;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

  

    public ProductsController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
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
            // Add at the top of your file
           

            // Replace this with your actual connection string
    var connectionString = _configuration["AzureBlobStorage:ConnectionString"];
           var containerName = _configuration["AzureBlobStorage:ContainerName"]; ; // e.g., "productimages"

            List<string> savedImageUrls = new List<string>();

            foreach (var image in product.ImageUrl.Split(','))
            {
                // You will receive the uploaded file here, instead of dealing with local paths
                // Assuming 'image' is the name of the uploaded file from the form (not the local file path)
                var fileStream = new MemoryStream(Convert.FromBase64String(image));  // If you're sending base64 string in your API

                // Create a BlobContainerClient for interacting with the blob container
                BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                string blobName = Path.GetFileName(image); // Use the file name from the request
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Upload the image to the Blob Storage
                await blobClient.UploadAsync(fileStream, overwrite: true);

                // Generate the URL of the uploaded image in Blob Storage
                string blobUrl = blobClient.Uri.ToString();

                // Add the image URL to the list
                savedImageUrls.Add(blobUrl);
            }

            product.ImageUrl = string.Join(",", savedImageUrls);
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
    public async Task<string> UploadToBlobAsync(IFormFile file)
    {
        var connectionString = _configuration["AzureBlobStorage:ConnectionString"];
        var containerName = _configuration["AzureBlobStorage:ContainerName"];

        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob); // optional for read

        var blobClient = containerClient.GetBlobClient(file.FileName);
        await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true);

        return blobClient.Uri.ToString();
    }

}
