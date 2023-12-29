using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.DbContext;

namespace TestWebApi_v1.Service.Hubs
{
    public class CustomUserIdProvider:IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var userIdClaim = connection.User?.FindFirst(ClaimTypes.NameIdentifier);

            // Lấy UserId từ claim
            var userId = userIdClaim?.Value;

            return userId;
        }
    }
}
