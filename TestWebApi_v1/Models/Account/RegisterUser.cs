using System.ComponentModel.DataAnnotations;

namespace TestWebApi_v1.Models.Account
{
    public class RegisterUser
    {
        [Required(ErrorMessage = "User Name is Required")]
        public string? UserName { get; set; } = null!;
        [Required(ErrorMessage = "Password is Required")]
        public string? Password { get; set; } = null!;
        [EmailAddress]
        [Required(ErrorMessage = "Email is Required")]
        public string? Email { get; set; }
        //public string? PhoneNumber { get; set; }
        //public string? Avatar { get; set; }
        //public string? Name { get; set; }
    }
}
