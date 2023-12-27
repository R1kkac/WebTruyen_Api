using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class BinhLuan
    {
        public BinhLuan()
        {
            ReplyComments = new HashSet<ReplyComment>();
        }

        public string IdComment { get; set; } = null!;
        public string IdUser { get; set; } = null!;
        public string MangaId { get; set; } = null!;
        public string? ChapterId { get; set; }
        public string? CommentData { get; set; }
        public int? Likecomment { get; set; }
        public int? Dislikecomment { get; set; }
        public DateTimeOffset? DateComment { get; set; }

        public virtual BoTruyen Manga { get; set; } = null!;
        public virtual User IdUserNavigation { get; set; } = null!;
        public virtual ICollection<ReplyComment> ReplyComments { get; set; }
    }
    public class danhSachBinhLuan
    {
        public string IdUser { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string UserAvatar { get; set; } = null!;
        public string IdComment { get; set; } = null!;
        public string MangaId { get; set; } = null!;
        public string? ChapterId { get; set; }
        public string? commentData { get; set; }
        public int? Likecomment { get; set; }
        public int? Dislikecomment { get; set; }
        public DateTimeOffset? date { get; set; }
    }
    public class danhSachReplyBinhLuan
    {
        public string IdUser { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string UserAvatar { get; set; } = null!;
        public string IdReply { get; set; } = null!;
        public string? NameReply { get; set; }
        public string? ReplyData { get; set; }
        public DateTimeOffset? DateReply { get; set; }
    }
}
