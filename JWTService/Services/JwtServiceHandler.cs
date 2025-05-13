using Grpc.Core;
using GrpcJwtService.Protos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWTService.Services
{
    public interface IJwtServiceHandler
    {
        Task<JwtReply> GenerateToken(JwtRequest request, ServerCallContext context);
    }
    public class JwtServiceHandler(IConfiguration configuration) :  JwtTokenService.JwtTokenServiceBase, IJwtServiceHandler
    {
        private readonly IConfiguration _configuration = configuration;

        public override Task<JwtReply> GenerateToken(JwtRequest request, ServerCallContext context)
        {
            try
            {
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!);
                var issuer = _configuration["Jwt:Issuer"]!;
                var audience = _configuration["Jwt:Audience"]!;
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, request.Userid), 
                    new(ClaimTypes.Name, request.Username),
                    new(ClaimTypes.Email, request.Email), 
                    new(ClaimTypes.Role, request.Role)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = issuer,
                    Audience = audience,
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };


                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                return Task.FromResult(new JwtReply
                {
                    Success = true,
                    Message = jwt
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new JwtReply
                {
                    Success = false,
                    Message = $"Token generation failed: {ex.Message}"
                });
            }
        }
      
    }
}
