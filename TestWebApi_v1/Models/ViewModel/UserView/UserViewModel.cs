namespace TestWebApi_v1.Models.ViewModel.UserView
{
    public class UserViewModel
    {
        public string? Id { get; set; }
        public string? Avatar { get; set; }
        public string? Name { get; set; }
        //public string? Email { get; set; }
        //public string? PhoneNumber { get; set; }
    }
    public class UserInfo
    {
        public string? Id { get; set; }
        public string? Avatar { get; set; }
        public string? Name { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? JoinDate { get; set; }
        public List<string>? Role { get; set; }
    }
}
