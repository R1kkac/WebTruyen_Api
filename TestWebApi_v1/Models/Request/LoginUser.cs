using System.ComponentModel.DataAnnotations;

namespace TestWebApi_v1.Models.Account
{
    public class LoginUser
    {
        [Required(ErrorMessage = "User Name Is Required!")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "User Name Is Required!")]
        public string? Password { get; set; }
    }
}
