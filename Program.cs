using ecommerce.Data;
using ecommerce.Repositories;
using ecommerce.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

// avoid default inbound claim type mapping which can rename role/name claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = jwtSection.GetValue<string>("Key");
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection.GetValue<string>("Issuer"),
            ValidAudience = jwtSection.GetValue<string>("Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? string.Empty)),
                    // ensure role claim is read from ClaimTypes.Role so Authorize(Roles="...") works
                    RoleClaimType = ClaimTypes.Role,
                    // map name identifier for convenience
                    NameClaimType = ClaimTypes.NameIdentifier
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed an admin user when running in Development (or when no admin exists).
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var config = services.GetRequiredService<IConfiguration>();
        var userRepo = services.GetRequiredService<ecommerce.Repositories.IUserRepository>();
        var userService = services.GetRequiredService<ecommerce.Services.IUserService>();

        // Wait for DB to be available (simple retry loop)
        const int maxAttempts = 10;
        var attempt = 0;
        IEnumerable<ecommerce.Entities.User> users = Enumerable.Empty<ecommerce.Entities.User>();
        while (attempt < maxAttempts)
        {
            try
            {
                users = userRepo.GetAllAsync().GetAwaiter().GetResult();
                break;
            }
            catch (Exception)
            {
                attempt++;
                Console.WriteLine($"Admin seeder: DB not ready, retrying ({attempt}/{maxAttempts})...");
                System.Threading.Thread.Sleep(2000);
            }
        }

        if (users == null || !users.Any())
        {
            // still no users; continue and try to create admin anyway
            users = Enumerable.Empty<ecommerce.Entities.User>();
        }

        var hasAdmin = users.Any(u => string.Equals(u.Role, "Admin", StringComparison.OrdinalIgnoreCase));
        if (!hasAdmin)
        {
            var adminEmail = config.GetValue<string>("Admin:Email") ?? "admin@example.com";
            var adminPassword = config.GetValue<string>("Admin:Password") ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin123!";

            var existing = userRepo.GetByEmailAsync(adminEmail.ToLowerInvariant()).GetAwaiter().GetResult();
            if (existing != null)
            {
                // promote existing user to Admin
                existing.Role = "Admin";
                userRepo.UpdateAsync(existing).GetAwaiter().GetResult();
                userRepo.SaveChangesAsync().GetAwaiter().GetResult();
            }
            else
            {
                var adminUser = new ecommerce.Entities.User
                {
                    FullName = "Administrador",
                    Email = adminEmail.ToLowerInvariant(),
                    PasswordHash = adminPassword,
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                // CreateUser will hash the password
                userService.CreateUser(adminUser).GetAwaiter().GetResult();
            }
        }
    }
    catch (Exception ex)
    {
        // don't crash the app on seeder failure; log to console
        Console.WriteLine($"Admin seeder error: {ex.Message}");
    }
}

app.Run("http://0.0.0.0:8080");
