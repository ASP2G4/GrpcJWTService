using Grpc.Core;
using GrpcJwtService.Protos;
using JWTService.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace JwtService.Tests
{
    public class JwtService_Test
    {
        private readonly Mock<IJwtServiceHandler> _jwtServiceHandlerMock;
        private readonly IJwtServiceHandler _jwtServiceHandler;

        public JwtService_Test()
        {
            _jwtServiceHandlerMock = new Mock<IJwtServiceHandler>();
            _jwtServiceHandler = _jwtServiceHandlerMock.Object;
        }

        [Fact]
        public async Task GenerateToken_ShouldReturnNotNullTrueAndSuccess()
        {
            // arrange
            var request = new JwtRequest
            {
                Userid = "test",
                Username = "test",
                Email = "test@test.com",
                Role = "test"
            };
            var expectedResponse = new JwtReply
            {
                Success = true,
                Message = "token"
            };
            _jwtServiceHandlerMock.Setup(x => x.GenerateToken(request, It.IsAny<ServerCallContext>()))
                .ReturnsAsync(expectedResponse);
            // act
            var result = await _jwtServiceHandler.GenerateToken(request, null);

            // assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("token", result.Message);
        }

        [Fact]
        public async Task GenerateToken_ShouldReturnAToken()
        {
            // arrange
            //Denna del har tagits fram med AI till request
            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:SecretKey", "your-very-strong-secret-key-1234567890123456"},
                {"Jwt:Issuer", "test-issuer"},
                {"Jwt:Audience", "test-audience"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var handler = new JwtServiceHandler(configuration);

            var request = new JwtRequest
            {
                Userid = "test",
                Username = "test",
                Email = "test@test.com",
                Role = "test"
            };

            // act
            var result = await handler.GenerateToken(request, null);

            // assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(string.IsNullOrWhiteSpace(result.Message));


            //Denna del har tagits fram med AI
            var handlerJwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            Assert.True(handlerJwt.CanReadToken(result.Message));
            var jwt = handlerJwt.ReadJwtToken(result.Message);
            Assert.Equal("test-issuer", jwt.Issuer);
            Assert.Contains(jwt.Claims, c => c.Type == "nameid" && c.Value == "test");
            Assert.Contains(jwt.Claims, c => c.Type == "unique_name" && c.Value == "test");
            Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == "test");
            Assert.Contains(jwt.Claims, c => c.Type == "email" && c.Value == "test@test.com");


        }
    }
}
