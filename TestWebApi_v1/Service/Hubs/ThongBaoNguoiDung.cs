using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MimeKit.Encodings;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.DbContext;
using Convert = TestWebApi_v1.Models.DbContext.Convert;
using AutoMapper;

namespace TestWebApi_v1.Service.Hubs
{
    public class ThongBaoNguoiDung
    {
        //    private readonly WebTruyenTranh_v2Context _context;
        //    private readonly IConfiguration _configuration;
        //    private readonly string _connectionString;
        //    private readonly IMapper _mapper;
        //    private static Dictionary<string, List<string>> UserConnect = new Dictionary<string, List<string>>();
        //    private static List<KenhChatUser>? CurrentGroupChat;
        //    public ThongBaoNguoiDung(WebTruyenTranh_v2Context context, IConfiguration configuration, IMapper mapper)
        //    {
        //        _context = context;
        //        _configuration = configuration;
        //        _connectionString= _configuration.GetConnectionString("DefaultConnection");
        //        CurrentGroupChat= new List<KenhChatUser>();
        //        _mapper = mapper;
        //    }
        //    public override async Task OnConnectedAsync()
        //    {
        //        //breakpoint here
        //        //some code
        //        //[...]
        //        var userconnect= Context.ConnectionId.ToString();
        //        Console.WriteLine("Ket noi hub voi id: "+ userconnect);
        //        //await ListRoomChat();
        //        await base.OnConnectedAsync();
        //    }
        //    public override async Task OnDisconnectedAsync(Exception? exception)
        //    {
        //        //breakpoint here
        //        //some code
        //        //[...]
        //        var userconnect = Context.ConnectionId.ToString();
        //        Console.WriteLine("Dung Ket noi hub voi id: "+ userconnect);
        //        //Xóa connectionId này khỏi toàn bộ group chat hiện hành
        //        await LeaveAllGroupChat(Context.ConnectionId);
        //        await base.OnDisconnectedAsync(exception);
        //    }
        //    //danh sách phòng chat
        //    public async Task ListRoomChat()
        //    {
        //        int input = 20;
        //        using (var connection = new SqlConnection(this._connectionString))
        //        {
        //            connection.Open();
        //            using (var command= new SqlCommand())
        //            {
        //                command.Connection = connection;
        //                command.CommandText = $"select top (@number) * from KenhChatUser where TinhTrang = 1";
        //                command.Parameters.Add(new SqlParameter("@number", SqlDbType.Int));
        //                command.Parameters["@number"].Value = input;

        //                //var data = new List<KenhChatUser>();
        //                using (var reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        //data.Add(new KenhChatUser
        //                        //{
        //                        //    Id = reader["Id"].ToString()!,
        //                        //    TenPhong = reader["TenPhong"].ToString()!,
        //                        //    NgayTao = reader.GetFieldValue<DateTimeOffset>("NgayTao"),
        //                        //    NgayDong = reader.IsDBNull(reader.GetOrdinal("NgayDong")) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>("NgayDong"),
        //                        //    TinhTrang = bool.Parse(reader["TinhTrang"].ToString()!),
        //                        //    NguoiTao = reader["NguoiTao"].ToString()!,
        //                        //});
        //                        CurrentGroupChat?.Add(new KenhChatUser
        //                        {
        //                            Id = reader["Id"].ToString()!,
        //                            TenPhong = reader["TenPhong"].ToString()!,
        //                            NgayTao = reader.GetFieldValue<DateTimeOffset>("NgayTao"),
        //                            NgayDong = reader.IsDBNull(reader.GetOrdinal("NgayDong")) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>("NgayDong"),
        //                            TinhTrang = bool.Parse(reader["TinhTrang"].ToString()!),
        //                            NguoiTao = reader["NguoiTao"].ToString()!,
        //                        });
        //                        //Console.WriteLine(data.Count());
        //                        //Console.WriteLine(CurrentGroupChat.Count());
        //                    }
        //                }
        //                await Clients.All.SendAsync("ListRommChat", CurrentGroupChat);
        //            }
        //        }
        //    }
        //    //tạo room chat
        //    public async Task CreateGroupChat(string iduser,string roomName)
        //    {
        //        if(roomName.Length <= 0)
        //        {
        //            return;
        //        }
        //        using (var connection= _context)
        //        {
        //            var roomId = Guid.NewGuid().ToString().Substring(0, 10);
        //            KenhChatUser kenhChatUser = new KenhChatUser
        //            {
        //                Id = roomId,
        //                TenPhong = roomName,
        //                NgayTao = DateTimeOffset.UtcNow,
        //                NgayDong = null,
        //                TinhTrang = true,
        //                NguoiTao = iduser
        //            };
        //            await connection.KenhChatUsers.AddAsync(kenhChatUser);
        //            await connection.SaveChangesAsync();
        //            GroupManager.AddUserToGroup(roomName, Context.ConnectionId, iduser);
        //            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        //            await ListRoomChat();
        //        }
        //    }
        //    //vào romm chat
        //    public async Task JoinGroupChat(string roomName,string iduser)
        //    {
        //        if(GroupManager.CheckuserExist(roomName, iduser) == false)
        //        {
        //            GroupManager.AddUserToGroup(roomName, Context.ConnectionId, iduser);
        //            using (var connection = _context)
        //            {
        //                KenhChatUser? IdRoomChat = await (connection.KenhChatUsers.FromSqlRaw($"select * from KenhChatUser where TenPhong like @p0", $"%{roomName}%").FirstOrDefaultAsync());
        //                UserRoomChat? userroom = await (from room in _context.KenhChatUsers
        //                                                join userroomchat in _context.UserRoomChats
        //                                                on room.Id equals userroomchat.IdChatRoom
        //                                                where userroomchat.IdUser == iduser && room.TenPhong == roomName
        //                                                select userroomchat).FirstOrDefaultAsync();
        //                if (userroom == null && IdRoomChat != null)
        //                {
        //                    UserRoomChat user = new UserRoomChat
        //                    {
        //                        Id = Guid.NewGuid().ToString().Substring(0, 6),
        //                        JoinTime = DateTimeOffset.UtcNow,
        //                        OutTime = null,
        //                        IdUser = iduser,
        //                        IdChatRoom = IdRoomChat.Id
        //                    };
        //                    IdRoomChat.UserRoomChats.Add(user);
        //                    await connection.SaveChangesAsync();
        //                }
        //                await Groups.AddToGroupAsync(Context.ConnectionId, IdRoomChat!.TenPhong);
        //                var ListId = GroupManager.GetListUserInGroup(roomName);
        //                foreach (var Id in ListId!)
        //                {
        //                    User? user = await _context.Users.FromSqlRaw($"select * from Users where Id = @p0", Id).FirstOrDefaultAsync();
        //                    if (user != null)
        //                    {
        //                        UserViewChat a = new UserViewChat
        //                        {
        //                            Id = user.Id,
        //                            Name = user.Name!,
        //                            Avatar = $"https://localhost:7132/Authentication/Avatar/{user.Avatar}"
        //                        };
        //                        await Clients.Group(IdRoomChat!.TenPhong).SendAsync("CurrentUser", a);
        //                        await Clients.Group(IdRoomChat!.TenPhong).SendAsync("Userjoinchat", a);
        //                    }
        //                }
        //                string? Host = Context.UserIdentifier;
        //                if (Host != null && Host == IdRoomChat.NguoiTao)
        //                {
        //                    var kenhchat = _mapper.Map<KenhChatUser2>(IdRoomChat);
        //                    await Clients.User(Context.UserIdentifier!).SendAsync("IsHostRoom", kenhchat);
        //                }
        //                await Clients.Group(IdRoomChat!.TenPhong).SendAsync("Notification", Context.User!.Identity!.Name + " has joined.");
        //                await Clients.Group(IdRoomChat!.TenPhong).SendAsync("Notification", $"Number of member in group are: {GroupManager.GetUsersCountInGroup(roomName)}");
        //            }
        //        }

        //    }
        //    //rời room chat
        //    public async Task LeaveGroupChat(string roomName, string iduser)
        //    {
        //        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        //        GroupManager.RemoveUserFromGroup(roomName, iduser);
        //        User? user = await _context.Users.FromSqlRaw($"select * from Users where Id = @p0",iduser).FirstOrDefaultAsync();
        //        if (user != null)
        //        {
        //            UserViewChat a = new UserViewChat
        //            {
        //                Id = user.Id,
        //                Name = user.Name!,
        //                Avatar = $"https://localhost:7132/Authentication/Avatar/{user.Avatar}"
        //            };
        //            await Clients.Group(roomName).SendAsync("UserLeaveChat", a);
        //        }
        //        await Clients.Group(roomName).SendAsync("Notification", Context.User!.Identity!.Name + " has leaved.");
        //        await Clients.Group(roomName).SendAsync("Notification", $"Number of member in group are: {GroupManager.GetUsersCountInGroup(roomName)}");
        //    }
        //    //Rởi khỏi toàn bộ group chat
        //    public async Task LeaveAllGroupChat(string connectionId)
        //    {
        //       using (var connection= _context)
        //        {
        //            string? iduser = GroupManager.getuserId(connectionId);
        //            if (iduser != null)
        //            {
        //                var user = await (from a in connection.Users where a.Id==iduser select 
        //                                  new UserViewChat
        //                                  {
        //                                      Id= a.Id,
        //                                      Avatar= $"https://localhost:7132/Authentication/Avatar/{a.Avatar}",
        //                                      Name=a.Name!,
        //                                  }).FirstOrDefaultAsync();
        //                var listGroup = GroupManager.GetListGroupFromUser(connectionId);
        //                foreach (var group in listGroup)
        //                {
        //                    await Groups.RemoveFromGroupAsync(connectionId, group);
        //                    await Clients.Group(group).SendAsync("UserLeaveChat", user);
        //                }
        //                GroupManager.RemoveUserFromAllGroup(connectionId);
        //            }
        //        }

        //    }
        //    //chat trong romm
        //    public async Task ChatToRoom(string roomName, string message, string IdUser)
        //    {
        //        using (var connection= _context)
        //        {
        //            UserRoomChat2? UserRoom =await (from room in _context.KenhChatUsers
        //                                      join userRoom in _context.UserRoomChats
        //                                      on room.Id equals userRoom.IdChatRoom
        //                                      join user in _context.Users
        //                                      on userRoom.IdUser equals user.Id
        //                                      where room.TenPhong == roomName && user.Id== IdUser select new UserRoomChat2
        //                                      {
        //                                          Id = userRoom.Id,
        //                                          IdChatRoom= room.Id,
        //                                          IdUser= user.Id,
        //                                          JoinTime= userRoom.JoinTime,
        //                                          OutTime= userRoom.OutTime,
        //                                          avatar= user.Avatar,
        //                                          userName= user.Name
        //                                      }).FirstOrDefaultAsync();
        //            if(UserRoom != null)
        //            {
        //                DataUserChat2 data = new DataUserChat2
        //                {
        //                    Id = Guid.NewGuid().ToString().Substring(0, 6),
        //                    IdUserChatRoom = UserRoom.Id,
        //                    Message = message,
        //                    Datetime = DateTimeOffset.UtcNow,
        //                    userName= UserRoom.userName,
        //                    avatar= $"https://localhost:7132/Authentication/Avatar/{UserRoom.avatar}"
        //                };
        //                _context.DataUserChats.Add(Convert.ConvertUserChat(data));
        //                await _context.SaveChangesAsync();
        //                //var user = Context.UserIdentifier;
        //                //Console.WriteLine("user Identity: " + user);
        //                //await Clients.Group(roomName).SendAsync("chatroom", $"{Context.User!.Identity!.Name}: {message}");
        //                await Clients.Group(roomName).SendAsync("chatroom", data); //data không có id

        //            }
        //        }
        //    }
        //    //Lấy nội dung chat của phòng chat
        //    public async Task GetDataChat(string idRoomname)
        //    {
        //        using (var connection= _context)
        //        {
        //            var room = await _context.KenhChatUsers.FindAsync(idRoomname)??null;
        //            if(room != null)
        //            {
        //                var useroomchat= await (from userroom in _context.UserRoomChats 
        //                                 join datachat in _context.DataUserChats
        //                                 on userroom.Id equals datachat.IdUserChatRoom
        //                                 where userroom.IdChatRoom == idRoomname
        //                                 select new DataUserChat2 {
        //                                     Id=datachat.Id,
        //                                     Datetime= datachat.Datetime,
        //                                     IdUserChatRoom= datachat.IdUserChatRoom,
        //                                     Message= datachat.Message,
        //                                     userName= userroom.IdUserNavigation.Name,
        //                                     avatar=$"https://localhost:7132/Authentication/Avatar/{userroom.IdUserNavigation.Avatar}",
        //                                 }).ToListAsync();
        //            foreach(var chat in useroomchat)
        //            {
        //                await Clients.Group(room.TenPhong).SendAsync("DataChatRoom", chat);
        //            }
        //                //await Clients.Group(room.TenPhong).SendAsync("DataChatRoom", useroomchat);
        //            }
        //        }
        //    }
        //    //Đóng kênh chat
        //    public async Task CloseGroupChat(string groupName, string userId)
        //    {
        //        var listConnectioId= GroupManager.GetlistConnectionId(groupName);
        //        if(listConnectioId != null)
        //        {
        //            using (var connection= _context)
        //            {
        //                var RoomChat = await connection.KenhChatUsers.FromSqlRaw($"select * from KenhChatUser where TenPhong like @p0", $"%{groupName}%")
        //                   .FirstOrDefaultAsync();
        //                foreach (var connectionId in listConnectioId)
        //                {
        //                    await Clients.Client(connectionId).SendAsync("CloseGroupChat", RoomChat);
        //                }
        //                if(RoomChat != null && RoomChat.NguoiTao==userId)
        //                {
        //                    RoomChat.TinhTrang = false;
        //                    await connection.SaveChangesAsync();
        //                }                 
        //           }
        //            GroupManager.RemoveGroupChat(groupName);
        //        }
        //    }
        //}
        //public class GroupManager
        //{
        //    private static Dictionary<string, List<UserChat>> groups = new Dictionary<string, List<UserChat>>();
        //    /// <summary>
        //    /// Thêm user vào group chat
        //    /// </summary>
        //    /// <param name="groupName"></param>
        //    /// <param name="connectionId"></param>
        //    /// <param name="userId"></param>
        //    public static void AddUserToGroup(string groupName,string connectionId, string userId)
        //    {
        //        if (!groups.ContainsKey(groupName))
        //        {
        //            groups[groupName] = new List<UserChat>();
        //        }

        //        if (!groups[groupName].Any(x=> x.userId == userId))
        //        {
        //            groups[groupName].Add(new UserChat { userId=userId, connectionId=connectionId});
        //        }
        //    }
        //    //xóa user khỏi một group {chuyển room, nếu dùng connectionid để xóa thì có nghĩa một uer có thể dùng nhiều thiết bị vào cùng một phòng}
        //    public static void RemoveUserFromGroup(string groupName, string userId)
        //    {
        //        if (groups.ContainsKey(groupName) && groups[groupName] !=null)
        //        {
        //            UserChat? user = groups[groupName].FirstOrDefault(x => x.userId == userId);
        //            if(user != null)
        //            {
        //                groups[groupName].Remove(user);
        //            }
        //            // Xóa group khỏi dictionary khi không còn user nào trong đó
        //            //if (groups[groupName].Count == 0)
        //            //{
        //            //    groups.Remove(groupName);
        //            //}
        //        }
        //    }
        //    /// <summary>
        //    /// Xóa group chat và toàn bộ user của nó
        //    /// </summary>
        //    /// <param name="connectionId"></param>
        //    /// <returns>True: False</returns>
        //    public static void RemoveGroupChat(string groupName)
        //    {
        //        if (groups.ContainsKey(groupName))
        //        {
        //            groups.Remove(groupName);
        //        }
        //    }
        //    //xóa user khỏi toàn bộ group {tắt trang web hay chuyển qua trang khác}
        //    public static void RemoveUserFromAllGroup(string connectionId)
        //    {
        //        var listGroup= groups.Where(x=> x.Value.Any(y=>y.connectionId == connectionId)).ToList();
        //        foreach(var group in listGroup)
        //        {
        //            UserChat user = group.Value.FirstOrDefault(x=>x.connectionId == connectionId)!;
        //            groups[group.Key].Remove(user);
        //        }
        //    }
        //    //Đếm số user trong group
        //    public static int GetUsersCountInGroup(string groupName)
        //    {
        //        if (groups.ContainsKey(groupName))
        //        {
        //            Console.WriteLine("Number of Froup: "+groups.Count().ToString());
        //            foreach(var a in groups)
        //            {
        //                Console.WriteLine(a);
        //            }
        //            return groups[groupName].Count;
        //        }

        //        return 0;
        //    }
        //    //Danh sách room chat
        //    public static Dictionary<string, List<UserChat>> ListRoomChat()
        //    {
        //        return groups;
        //    }
        //    /// <summary>
        //    /// lấy danh sách iduser hiện đang có trong group chat
        //    /// </summary>
        //    /// <param name="groupName"></param>
        //    /// <returns>List: Iduser(string)|| null</returns>
        //    public static List<string>? GetListUserInGroup(string groupName)
        //    {
        //        if (groups.ContainsKey(groupName))
        //        {
        //            return groups[groupName].Select(x=>x.userId).ToList();
        //        }
        //        return null;
        //    }
        //    /// <summary>
        //    /// lấy danh sách connectionId hiện đang có trong group chat
        //    /// </summary>
        //    /// <param name="groupName"></param>
        //    /// <returns>List: connectionId(string)|| null</returns>
        //    public static List<string>? GetlistConnectionId(string groupName)
        //    {
        //        if (groups.ContainsKey(groupName))
        //        {
        //            return groups[groupName].Select(x => x.connectionId).ToList();
        //        }
        //        return null;
        //    }
        //    /// <summary>
        //    /// Lấy danh sách group mà user vào
        //    /// </summary>
        //    /// <param name="connectionId"></param>
        //    /// <returns>`listrroom: list'string'</returns>
        //    public static List<string> GetListGroupFromUser(string connectionId)
        //    {
        //        return groups.Where(x=>x.Value.Any(y=> y.connectionId == connectionId)).Select(x=>x.Key).ToList();
        //    }
        //    /// <summary>
        //    /// lấy user id khi biết connection id
        //    /// </summary>
        //    /// <param name="connectionId"></param>
        //    /// <returns>idUser: string</returns>
        //    public static string? getuserId(string connectionId)
        //    {
        //        var group = groups.FirstOrDefault(x => x.Value.Any(y => y.connectionId == connectionId));
        //        if (group.Key != null)
        //        {
        //            var user = group.Value.FirstOrDefault(z => z.userId != null);
        //            if (user != null)
        //            {
        //                return user.userId;
        //            }
        //        }
        //        return null;
        //        //cách 2
        //        //return groups.FirstOrDefault(x => x.Value.Any(y => y.connectionId == connectionId)).Value?.FirstOrDefault(z => z.userId != null)!.userId.ToString();
        //    }
        //    /// <summary>
        //    /// Kiểm tra xem User đó co trong room chưa
        //    /// </summary>
        //    public static bool CheckuserExist(string groupName, string userId)
        //    {
        //        if (groups.ContainsKey(groupName))
        //        {
        //            if (groups[groupName].Any(x=> x.userId == userId))
        //            {
        //                return true;
        //            }
        //        }
        //        return false;
        //    }
        //    public class UserChat
        //    {
        //        public string connectionId { get; set; } = null!;
        //        public string userId { get; set; } = null!;
        //    }

        //}
    }
}
