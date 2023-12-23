using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class Datachat
    {
        public int ChatId { get; set; }
        public string RoomId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string? Message { get; set; }
        public DateTimeOffset? TimeChat { get; set; }

        public virtual UserJoinChat UserJoinChat { get; set; } = null!;
    }
}
