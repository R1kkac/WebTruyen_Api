using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class ReplyComment
    {
        public string IdReply { get; set; } = null!;
        public string? IdComment { get; set; }
        public string? IdUserReply { get; set; }
        public string? Replydata { get; set; }
        public DateTimeOffset? DateReply { get; set; }

        public virtual BinhLuan? IdCommentNavigation { get; set; }
        public virtual User? IdUserReplyNavigation { get; set; }
    }
}
