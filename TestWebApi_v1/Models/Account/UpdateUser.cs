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
}
