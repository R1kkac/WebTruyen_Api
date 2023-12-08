using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public class DataUserChatBase
    {
        public string Id { get; set; } = null!;
        public string IdUserChatRoom { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTimeOffset Datetime { get; set; }
    }
    public partial class DataUserChat: DataUserChatBase
    {
        public virtual UserRoomChat? IdUserChatRoomNavigation { get; set; }
    }
    public partial class DataUserChat2 : DataUserChatBase
    {
        public string? userName { get; set; }
        public string? avatar { get; set; }
    }
    public class Convert
    {
        public static DataUserChat ConvertUserChat(DataUserChat2 data)
        {
            return new DataUserChat { Id = data.Id, Datetime = data.Datetime, IdUserChatRoom = data.IdUserChatRoom, Message = data.Message };
        }
    }
}
