using System.ComponentModel.DataAnnotations;

namespace TestWebApi_v1.Models.Account
{
    public class UpdateUser
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is Required")]
        public string? Email { get; set; }
        public string? OldPassword { get; set; } = null!;
        public string? NewPassword { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public string? Name { get; set; }
    }
    public class EditUser
    {
        public string email { get; set; }= null!;
        public string? name { get;set; }
        public string? phone { get; set; }
    }
    public class changePasswordUser
    {
        public string email { get; set; } = null!;
        public string? oldpassword { get; set; } = null!;
        public string? newpassword { get; set; } = null!;
    }
}
