using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class Role : IdentityRole
    {
        public Role()
        {
            UserRoles = new HashSet<UserRoles>();
        }

        public virtual ICollection<UserRoles> UserRoles { get; set; }
    }
}
