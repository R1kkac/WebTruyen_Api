namespace TestWebApi_v1.Models.DbContext
{
    public class ThongbaoUser
    {
        public string Id { get; set; } = null!;
        public string IdUser { get; set; }=null!;
        public bool seen { get; set; }
        public string message { get; set; } = null!;
        public DateTime dateTime { get; set; }
        public string target { get; set; } = null!;

        public virtual User IdNavigation { get; set; } = null!;
    }
}
