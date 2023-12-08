using Microsoft.AspNetCore.Identity;

namespace TestWebApi_v1.Models.DbContext
{
    public class UserRoles : IdentityUserRole<string>
    {
        public virtual User user { get; set; } = null!;
        public virtual Role role { get; set; } = null!;
    }
}
