using ecommerce.DTOs;
using ecommerce.Entities;
using ecommerce.Repositories;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BCrypt.Net;

namespace ecommerce.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existing = await _repo.GetByEmailAsync(request.Email);
            if (existing != null) throw new ApplicationException("Email already in use");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email.ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(user);
            await _repo.SaveChangesAsync();

            return GenerateToken(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email.ToLowerInvariant());
            if (user == null) throw new ApplicationException("Invalid credentials");

            var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!valid) throw new ApplicationException("Invalid credentials");

            return GenerateToken(user);
        }

        private AuthResponse GenerateToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key");
            var issuer = jwtSection.GetValue<string>("Issuer");
            var audience = jwtSection.GetValue<string>("Audience");
            var expiresMinutes = jwtSection.GetValue<int>("ExpiresMinutes");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
      {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Role, user.Role)
      };

            var token = new JwtSecurityToken(
              issuer: issuer,
              audience: audience,
              claims: claims,
              expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
              signingCredentials: creds
            );

            var tokenString = tokenHandler.WriteToken(token);
            return new AuthResponse(tokenString, "Bearer", token.ValidTo);
        }
    }
}
