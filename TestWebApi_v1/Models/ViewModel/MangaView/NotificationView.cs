namespace TestWebApi_v1.Models.ViewModel.MangaView
{
    public class NotificationView
    {
        public string? Id { get; set; } = null!;
        public string? IdUser { get; set; }
        public bool seen { get; set; }
        public string? message { get; set; }
        public DateTime dateTime { get; set; }
        public string? target { get; set; }
        public string? nametarget { get; set; }
        public string? imagerarget { get; set; }

    }
}
