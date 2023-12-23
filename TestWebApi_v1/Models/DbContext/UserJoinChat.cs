using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class UserJoinChat
    {
        public UserJoinChat()
        {
            Datachats = new HashSet<Datachat>();
        }

        public string RoomId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public bool? Status { get; set; }

        public virtual ChatRoom Room { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Datachat> Datachats { get; set; }
    }
}
