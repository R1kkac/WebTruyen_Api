namespace TestWebApi_v1.Models.ViewModel.UserView
{
    public class CommentViewModel
    {
        public string IdComment { get; set; } = null!;
        public string IdUser { get; set; } = null!;
        public string ChapterId { get; set; } = null!;
        public string? CommentData { get; set; }
        public DateTimeOffset? DateComment { get; set; }
        public string? CurChapter { get;set; }
        public string? Name { get; set; }
        public string? Avatar { get; set; }
    }
}
