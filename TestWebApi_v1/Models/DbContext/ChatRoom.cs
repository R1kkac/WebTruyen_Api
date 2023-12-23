using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class ChatRoom
    {
        public ChatRoom()
        {
            UserJoinChats = new HashSet<UserJoinChat>();
        }

        public string RoomId { get; set; } = null!;
        public string MangaId { get; set; } = null!;
        public DateTimeOffset? TimeStart { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public bool? Status { get; set; }

        public virtual BoTruyen Manga { get; set; } = null!;
        public virtual ICollection<UserJoinChat> UserJoinChats { get; set; }
    }
}
