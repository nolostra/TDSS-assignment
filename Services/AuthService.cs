using LinenManagementSystem.Data;
using LinenManagementSystem.DTOs.Auth;
using LinenManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LinenManagementSystem.Services
{
    public interface IAuthenticationService
    {
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginRequest);
        Task<bool> LogoutAsync(string refreshToken);
        Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
    }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto loginRequest)
        {
            // Validate credentials
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == loginRequest.Email && e.Password == loginRequest.Password);

            if (employee == null)
            {
                return null;
            }

            // Generate tokens
            var accessToken = GenerateJwtToken(employee);
            var refreshToken = GenerateRefreshToken();

            // Save refresh token in the database
            employee.RefreshToken = refreshToken;
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.RefreshToken == refreshToken);

            if (employee == null)
            {
                return false;
            }

            // Invalidate the refresh token
            employee.RefreshToken = null;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.RefreshToken == refreshToken);

            if (employee == null)
            {
                return null;
            }

            // Generate new tokens
            var newAccessToken = GenerateJwtToken(employee);
            var newRefreshToken = GenerateRefreshToken();

            // Update refresh token in the database
            employee.RefreshToken = newRefreshToken;
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        // Helper method to generate JWT token
        private string GenerateJwtToken(Employees employee)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]); // Define in appsettings.json

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                    new Claim(ClaimTypes.Email, employee.Email),
                }),
                Expires = DateTime.UtcNow.AddMinutes(15), // Access token expiration
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Helper method to generate a refresh token
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
