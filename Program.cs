using EComAPI.Data;
using EComAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();
byte[] key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]);
var jwtKey = builder.Configuration["JwtSettings:Key"];
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Logging.ClearProviders();




var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.WebHost.UseUrls("http://*:5000", "https://*:5001");
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            //policy.WithOrigins("http://localhost:4200")
            policy.WithOrigins("https://ecom-api-test-e5g9ccfwfjdufyh8.southeastasia-01.azurewebsites.net")// Allow Angular frontend
                  .AllowAnyMethod()
                  .AllowAnyHeader()
            .AllowCredentials();
        });
});
builder.Services.AddIdentity<User, IdentityRole>()
       .AddEntityFrameworkStores<AppDbContext>()
       .AddDefaultTokenProviders();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Set true in production
        options.SaveToken = true;



        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],

            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"Unauthorized: {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
   //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
   ////options.UseSqlServer("Server=ecom-sql-server123.database.windows.net,1433;Initial Catalog=EComDB;User ID=sqladmin;Password=nikki@123;Encrypt=True;TrustServerCertificate=True;")
   // );

    options.UseSqlServer(connStr));

builder.Services.AddControllers();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer <your_token>'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
});
});
//builder.Services.AddAuthorization();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
});
builder.Services.AddControllers();

var app = builder.Build();
//app.Use(async (context, next) =>
//{
//    try
//    {
//        await next();
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Unhandled Exception: {ex.Message}");
//        throw;
//    }
//});
app.UseCors(MyAllowSpecificOrigins);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//var key1 = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]));
//var credentials = new SigningCredentials(key1, SecurityAlgorithms.HmacSha256);

//var tokenDescriptor = new SecurityTokenDescriptor
//{
//    Expires = DateTime.UtcNow.AddHours(1),
//    SigningCredentials = credentials
//};

//var tokenHandler = new JwtSecurityTokenHandler();
//var token = tokenHandler.CreateToken(tokenDescriptor);
//var tokenString = tokenHandler.WriteToken(token);

//// ? Print token in console
//Console.WriteLine($"Generated Token in Program.cs-144/03: {tokenString}");
Console.WriteLine($"JWT Key: {builder.Configuration["JwtSettings:Key"]}");
Console.WriteLine($"JWT Key in Program.cs: {jwtKey}");
Console.WriteLine("?? Connection String from config: " + connStr);
app.UseDeveloperExceptionPage();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.Run();

