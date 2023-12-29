namespace TestWebApi_v1.Models.ViewModel.UserView
{
    public class ResponeComment
    {
        public string IdComment { get; set; } = null!;
        public string IdUser { get; set; } = null!;
        public string? ChapterId { get; set; }
        public string? CommentData { get; set; }
        public DateTimeOffset? DateComment { get; set; }
        public string? CurChapter { get;set; }
        public string? Name { get; set; }
        public string? Avatar { get; set; }
        public int? Likecomment { get; set; }
        public int? Dislikecomment { get; set; }
    }
}
