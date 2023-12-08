using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace TestWebApi_v1.Models.Account
{

    public class CheckToken:ICheckToken
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public CheckToken(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }
        public bool CheckTokenDate()
        {
            string token = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"]
           .ToString().Replace("Bearer ", "");

            // Giải mã token để kiểm tra thông tin
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                RequireExpirationTime = true,
                ValidIssuer = _configuration["JWT:ValidIssuer"],
                ValidAudience = _configuration["JWT:ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]))
            };

            SecurityToken validatedToken;
            var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);

            if (validatedToken.ValidTo > DateTime.UtcNow)
            {
                return true;
            }
            return false;
        }
    }
}
