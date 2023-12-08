using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public class UserRoomChatBase
    {
        public string Id { get; set; } = null!;
        public DateTimeOffset? JoinTime { get; set; }
        public DateTimeOffset? OutTime { get; set; }
        public string IdUser { get; set; } = null!;
        public string IdChatRoom { get; set; } = null!;
    }
    public partial class UserRoomChat: UserRoomChatBase
    {
        public UserRoomChat()
        {
            DataUserChats = new HashSet<DataUserChat>();
        }
        public virtual KenhChatUser IdChatRoomNavigation { get; set; } = null!;
        public virtual User IdUserNavigation { get; set; } = null!;
        public virtual ICollection<DataUserChat> DataUserChats { get; set; }
    }
    public partial class UserRoomChat2: UserRoomChatBase
    {
        public string? userName { get; set; }
        public string? avatar { get; set; }
    }
}
