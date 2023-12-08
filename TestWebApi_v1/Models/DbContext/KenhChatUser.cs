using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class KenhChatUser
    {
        public KenhChatUser()
        {
            UserRoomChats = new HashSet<UserRoomChat>();
        }

        public string Id { get; set; } = null!;
        public string TenPhong { get; set; } = null!;
        public DateTimeOffset NgayTao { get; set; }
        public DateTimeOffset? NgayDong { get; set; }
        public bool TinhTrang { get; set; }
        public string NguoiTao { get; set; } = null!;

        public virtual User NguoiTaoNavigation { get; set; } = null!;
        public virtual ICollection<UserRoomChat> UserRoomChats { get; set; }
    }
    public class KenhChatUser2 {
        public string Id { get; set; } = null!;
        public string TenPhong { get; set; } = null!;
        public DateTimeOffset NgayTao { get; set; }
        public DateTimeOffset? NgayDong { get; set; }
        public bool TinhTrang { get; set; }
        public string NguoiTao { get; set; } = null!;
    }
}
