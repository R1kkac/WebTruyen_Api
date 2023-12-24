using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.ViewModel.UserView;

namespace TestWebApi_v1.Service.Hubs
{
    [Authorize]
    public class ChatRealTime: Hub
    {
        private readonly WebTruyenTranh_v2Context _db;
        public ChatRealTime(WebTruyenTranh_v2Context db) 
        {
            _db = db;
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
        public async Task listRoomChatActive()
        {
            //ChatManager.testPushRoomChat();
            var list = ChatManager.ListRoomChat();
            await Clients.All.SendAsync("list_room_chat_active", list);
        }
        public async Task createChatRoom(string room)
        {
            try
            {
                var randomId = Guid.NewGuid().ToString().Substring(0, 6);
                var Room= JsonSerializer.Deserialize<RoomChatPost>(room);
                var checkRoomExits=await _db.ChatRooms.AnyAsync(x=> x.RoomId.Equals(Room!.MangaId) && x.Status ==true);
                if(checkRoomExits == true)
                {
                    await Clients.User(ClaimTypes.NameIdentifier).SendAsync("notification", "Phòng đã tồn tại");
                }
                else
                {
                    ChatRoom a = new ChatRoom
                    {
                        RoomId = randomId,
                        MangaId = Room!.MangaId,
                        TimeStart = DateTime.UtcNow,
                        EndTime = null,
                        Status = true,
                    };
                    await _db.ChatRooms.AddAsync(a);
                    await _db.SaveChangesAsync();
                    RoomChat x = new RoomChat
                    {
                        RoomId = a.RoomId,
                        RoomName = Room.RoomName,
                        image = Room.image,
                        MangaId= Room.MangaId
                    };
                    ChatManager.NewRoomChat(x);
                    await listRoomChatActive();
                    await Clients.User(Context.UserIdentifier!).SendAsync("notification", "Tạo phòng thành công");
                }
            }
            catch(Exception err)
            {
                Console.WriteLine(err);
            }
        }
        public async Task userJoinChatRoom(string userdata, string roomId)
        {
            var user = JsonSerializer.Deserialize<UserViewModel>(userdata);
            var roomExits = ChatManager.CheckExitsRoomChat(roomId);
            if(roomExits == false)
            {
                await Clients.User(ClaimTypes.NameIdentifier).SendAsync("notification", "Phòng không tồn tại");
            }
            else
            {
                if(ChatManager.CheckUserInRoom(user!.Id!, roomId) == null)
                {
                    var Room = await _db.ChatRooms.Where(x => x.RoomId.Equals(roomId) && x.Status == true)
                        .Include(y=> y.UserJoinChats)
                        .SingleOrDefaultAsync(); ;
                    if(Room != null)
                    {
                        UserJoinChat a = new UserJoinChat
                        {
                            UserId = user!.Id!,
                            RoomId = roomId,
                            Status = true,
                        };
                        if(Room.UserJoinChats.Any(x=> x.RoomId.Equals(a.RoomId) && x.UserId.Equals(a.UserId)) == false)
                        {
                            Room.UserJoinChats.Add(a);
                            await _db.SaveChangesAsync();
                            user_chat z = new user_chat
                            {
                                id = user.Id!,
                                name = user.Name!,
                                avatar = user.Avatar,
                                messages = new List<message_chat>(),
                            };
                            ChatManager.AddUserToRoom(roomId, z);
                            var data = System.Text.Json.JsonSerializer.Serialize(a, new JsonSerializerOptions
                            {
                                ReferenceHandler = ReferenceHandler.Preserve
                            });
                            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                            await Clients.Group(roomId).SendAsync("user_in_room", z);
                        }
                        else
                        {
                            var userinroom = Room.UserJoinChats.Where(x => x.RoomId.Equals(roomId) && x.UserId.Equals(user.Id)).SingleOrDefault();
                            if (userinroom != null)
                            {
                                userinroom.Status = true;
                                await _db.SaveChangesAsync();
                                user_chat z = new user_chat
                                {
                                    id = user.Id!,
                                    name = user.Name!,
                                    avatar = user.Avatar,
                                    messages = new List<message_chat>(),
                                };
                                ChatManager.AddUserToRoom(roomId, z);
                                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                                await Clients.Group(roomId).SendAsync("user_in_room", z);


                            }
                        }

                    }                    
                }               
            }
        }
        public async Task userLeaveChatRoom(string userId, string roomId)
        {
            var user = await _db.ChatRooms
                 .Where(x => x.RoomId.Equals(roomId))
                 .Include(y => y.UserJoinChats)
                 .Select(z => z.UserJoinChats.FirstOrDefault(h => h.UserId.Equals(userId)))
                 .SingleOrDefaultAsync();
            if (user != null)
            {
                user.Status = false;
                await _db.SaveChangesAsync();
                var userInRoom = ChatManager.FindUserInRoom(roomId, user.UserId);
                if (userInRoom != null)
                {
                    await Clients.Groups(roomId).SendAsync("cur_users_leave_room", userInRoom);
                    ChatManager.RemoveUserFromGroup(roomId, userId);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                }    
            }
        }
        public async Task GetCurUserInRoom(string roomId)
        {
            var Users = ChatManager.CurrentUsersInRoom(roomId);
            await Clients.Groups(roomId).SendAsync("cur_users_in_room", Users);

        }
    }
    public class ChatManager
    {
        private static Dictionary<string, List<RoomChat>> Rooms = new Dictionary<string, List<RoomChat>>();
        public static Dictionary<string, List<user_chat>> UsersChat = new Dictionary<string, List<user_chat>>();
        private static string ListRoomName = "Rooms";
        public static bool NewRoomChat(RoomChat room)
        {
            if (!Rooms.ContainsKey(ListRoomName))
            {
                Rooms.Add(ListRoomName, new List<RoomChat>());
                Rooms[ListRoomName].Add(room);
                return true;
            }
            else
            {
                if (!Rooms.Any(x => x.Value.Any(y => y.RoomId.Equals(room.RoomId))))
                {
                    Rooms[ListRoomName].Add(room);
                    return true;
                }
                else return false;
            }
           
        }
        //Danh sách room chat
        public static Dictionary<string, List<RoomChat>> ListRoomChat()
        {
            return Rooms;
        }
        public static bool CheckExitsRoomChat(string roomId)
        {
            if (Rooms.ContainsKey(ListRoomName))
            {
                return Rooms[ListRoomName].Any(x => x.RoomId.Equals(roomId));
            }
            return false;
        }
        public static void testPushRoomChat()
        {
            var room1 = new RoomChat()
            {
                RoomId = "306142",
                RoomName = "Re:Zero kara Hajimeru Isekai Seikatsu",
                image="",
            };
            var room2 = new RoomChat()
            {
                RoomId = "119067",
                RoomName = "Sakurasou",
                image = "",
            };
            NewRoomChat(room1);
            NewRoomChat(room2);

        }


















        /// <summary>
        /// Thêm user vào room chat
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="connectionId"></param>
        /// <param name="userId"></param>
        public static void AddUserToRoom(string roomId, user_chat user)
        {
            if (!UsersChat.ContainsKey(roomId))
            {
                UsersChat.Add(roomId, new List<user_chat>());
                UsersChat[roomId].Add(user);
            }

            if (!UsersChat[roomId].Any(x => x.id == user.id))
            {
                UsersChat[roomId].Add(user);
            }
        }
        public static user_chat? CheckUserInRoom(string userId, string roomId)
        {
            if (UsersChat.TryGetValue(roomId, out var userInRoom))
            {
                return userInRoom.Where(x => x.id.Equals(userId)).SingleOrDefault();
            }
            return null;
        }
        public static List<user_chat>? CurrentUsersInRoom(string roomId)
        {
            if(UsersChat.TryGetValue(roomId,out var ListUsers))
            {
                return ListUsers;
            }
            return null;
        }
        ////xóa user khỏi một group {chuyển room, nếu dùng connectionid để xóa thì có nghĩa một uer có thể dùng nhiều thiết bị vào cùng một phòng}
        public static void RemoveUserFromGroup(string roomId, string userId)
        {
            if (UsersChat.ContainsKey(roomId) && UsersChat[roomId] != null)
            {
                user_chat? user = UsersChat[roomId].SingleOrDefault(x => x.id.Equals(userId));
                if (user != null)
                {
                    UsersChat[roomId].Remove(user);
                }
                //Xóa group khỏi dictionary khi không còn user nào trong đó
                if (UsersChat[roomId].Count == 0)
                {
                    UsersChat.Remove(roomId);
                }
            }
        }
        public static user_chat? FindUserInRoom(string roomId, string userId)
        {
            if(UsersChat.TryGetValue(roomId, out var litRoom))
            {
                return UsersChat[roomId].SingleOrDefault(x => x.id.Equals(userId));
            }
            return null;
        }
        ///// <summary>
        ///// Xóa room chat và toàn bộ user của nó
        ///// </summary>
        ///// <param name="connectionId"></param>
        ///// <returns>True: False</returns>
        //public static void RemoveGroupChat(string groupName)
        //{
        //    if (Rooms.ContainsKey(groupName))
        //    {
        //        Rooms.Remove(groupName);
        //    }
        //}
        ////xóa user khỏi toàn bộ room {tắt trang web hay chuyển qua trang khác}
        //public static void RemoveUserFromAllGroup(string connectionId)
        //{
        //    var listGroup = Rooms.Where(x => x.Value.Any(y => y.connectionId == connectionId)).ToList();
        //    foreach (var group in listGroup)
        //    {
        //        UserChat user = group.Value.FirstOrDefault(x => x.connectionId == connectionId)!;
        //        Rooms[group.Key].Remove(user);
        //    }
        //}
        ////Đếm số user trong room
        //public static int GetUsersCountInGroup(string groupName)
        //{
        //    if (Rooms.ContainsKey(groupName))
        //    {
        //        Console.WriteLine("Number of Froup: " + Rooms.Count().ToString());
        //        foreach (var a in Rooms)
        //        {
        //            Console.WriteLine(a);
        //        }
        //        return Rooms[groupName].Count;
        //    }

        //    return 0;
        //}
        ///// <summary>
        ///// lấy danh sách iduser hiện đang có trong room chat
        ///// </summary>
        ///// <param name="groupName"></param>
        ///// <returns>List: Iduser(string)|| null</returns>
        //public static List<string>? GetListUserInGroup(string groupName)
        //{
        //    if (Rooms.ContainsKey(groupName))
        //    {
        //        return Rooms[groupName].Select(x => x.userId).ToList();
        //    }
        //    return null;
        //}
        ///// <summary>
        ///// lấy danh sách connectionId hiện đang có trong room chat
        ///// </summary>
        ///// <param name="groupName"></param>
        ///// <returns>List: connectionId(string)|| null</returns>
        //public static List<string>? GetlistConnectionId(string groupName)
        //{
        //    if (Rooms.ContainsKey(groupName))
        //    {
        //        return Rooms[groupName].Select(x => x.connectionId).ToList();
        //    }
        //    return null;
        //}
        ///// <summary>
        ///// Lấy danh sách room mà user vào
        ///// </summary>
        ///// <param name="connectionId"></param>
        ///// <returns>`listrroom: list'string'</returns>
        //public static List<string> GetListGroupFromUser(string connectionId)
        //{
        //    return Rooms.Where(x => x.Value.Any(y => y.connectionId == connectionId)).Select(x => x.Key).ToList();
        //}
        ///// <summary>
        ///// lấy user id khi biết connection id
        ///// </summary>
        ///// <param name="connectionId"></param>
        ///// <returns>idUser: string</returns>
        //public static string? getuserId(string connectionId)
        //{
        //    var group = Rooms.FirstOrDefault(x => x.Value.Any(y => y.connectionId == connectionId));
        //    if (group.Key != null)
        //    {
        //        var user = group.Value.FirstOrDefault(z => z.userId != null);
        //        if (user != null)
        //        {
        //            return user.userId;
        //        }
        //    }
        //    return null;
        //    //cách 2
        //    //return groups.FirstOrDefault(x => x.Value.Any(y => y.connectionId == connectionId)).Value?.FirstOrDefault(z => z.userId != null)!.userId.ToString();
        //}
        ///// <summary>
        ///// Kiểm tra xem User đó co trong room chưa
        ///// </summary>
        //public static bool CheckuserExist(string groupName, string userId)
        //{
        //    if (Rooms.ContainsKey(groupName))
        //    {
        //        if (Rooms[groupName].Any(x => x.userId == userId))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
    //chia ra làm 2 dictionary 1 save romm chat và 1 save datachat
    //datachat chứa idroomchat và data chứa userchat lưu iduser và message_chat
    public class RoomChat
    {
        public string RoomId { get; set; } = null!;
        public string MangaId { get; set; } = null!;
        public string RoomName { get; set; } = null!;
        public string? image { get; set; }
    }
    public class RoomChatPost
    {
        public string MangaId { get; set; } = null!;
        public string RoomName { get; set; } = null!;
        public string? image { get; set; }
    }
    public class user_chat
    {
        public string id { get; set; } = null!;
        public string name { get; set; } = null!;
        public string? avatar { get; set; }
        public List<message_chat>? messages { get; set; }
    }
    public class message_chat
    {
        public string? data_chat { get; set; }
        public string? time_chat { get; set; }
    }
}
