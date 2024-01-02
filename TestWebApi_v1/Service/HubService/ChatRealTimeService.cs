using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.ViewModel.UserView;

namespace TestWebApi_v1.Service.Hubs
{
    [Authorize]
    public class ChatRealTimeService: Hub
    {
        private readonly WebTruyenTranh_v2Context _db= new WebTruyenTranh_v2Context();
        public override async Task OnConnectedAsync()
        {
            await Clients.User(Context.UserIdentifier!).SendAsync("isdisconnect", false);
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var listRoom = ChatManager.UsersChat.Where(x => x.Value.Any(y => y.id.Equals(Context.UserIdentifier)));
            foreach( var room in listRoom)
            {
                await userLeaveChatRoom(Context.UserIdentifier!, room.Key, Context.ConnectionId);
            }
            //await Clients.All.SendAsync("cur_user_in_room", ChatManager.numBerUserInRoom());
            await Clients.User(Context.UserIdentifier!).SendAsync("isdisconnect", true) ;
            await base.OnDisconnectedAsync(exception);
        }
        public async Task listRoomChatActive()
        {
            var list = ChatManager.ListRoomChat();
            await Clients.All.SendAsync("cur_user_in_room", ChatManager.numBerUserInRoom());
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
                    return;
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
            var user = JsonSerializer.Deserialize<ResponeUser>(userdata);
            var roomExits = ChatManager.CheckExitsRoomChat(roomId);
            var findUserinRoom = ChatManager.FindUserInAnyRoom(Context.ConnectionId, user!.Id!);
            if (findUserinRoom != null && findUserinRoom.Any())
            {
                foreach (var curRoomId in findUserinRoom)
                {
                    await userLeaveChatRoom(user!.Id!, curRoomId, Context.ConnectionId);
                }
            }
            //if (ChatManager.FindUserInRoom(roomId, user!.Id!, Context.ConnectionId) != null)
            //{
            //    await userLeaveChatRoom(user!.Id!, roomId, Context.ConnectionId);
            //}
            if (ChatManager.CheckUserExitsInAnyRoom(user!.Id!, Context.ConnectionId) > 0)
            {
                await Clients.Clients(Context.ConnectionId).SendAsync("notification",
                       "Bạn đã tham gia kênh chat này ở một nơi khác");
                //await Clients.Clients(Context.ConnectionId!).SendAsync("is_join_room", ChatManager.CheckUserExitsInAnyRoom(user!.Id!));
                return;
            }
            if (!roomExits)
            {
                await Clients.User(ClaimTypes.NameIdentifier).SendAsync("notification", "Phòng không tồn tại");
                return;
            }
            else
            {
                var Room = await _db.ChatRooms.Where(x => x.RoomId.Equals(roomId) && x.Status == true)
                       .Include(y => y.UserJoinChats)
                       .SingleOrDefaultAsync(); ;
                if (Room != null)
                {
                    UserJoinChat a = new UserJoinChat
                    {
                        UserId = user!.Id!,
                        RoomId = roomId,
                        Status = true,
                    };
                    if (Room.UserJoinChats.Any(x => x.RoomId.Equals(a.RoomId) && x.UserId.Equals(a.UserId)) == false)
                    {
                        Room.UserJoinChats.Add(a);
                        await _db.SaveChangesAsync();
                        user_chat z = new user_chat
                        {
                            conId = Context.ConnectionId,
                            id = user.Id!,
                            name = user.Name!,
                            avatar = user.Avatar,
                            status = true,
                            messages = new List<message_chat>(),
                        };
                        ChatManager.AddUserToRoom(roomId, z);
                        var data = System.Text.Json.JsonSerializer.Serialize(a, new JsonSerializerOptions
                        {
                            ReferenceHandler = ReferenceHandler.Preserve
                        });
                        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                        var Users = ChatManager.CurrentUsersInRoom(roomId);
                        await Clients.Groups(roomId).SendAsync("cur_users_in_room", Users);
                        await Clients.Group(roomId).SendAsync("user_in_room", z);
                        await Clients.All.SendAsync("cur_user_in_room", ChatManager.numBerUserInRoom());
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
                                conId = Context.ConnectionId,
                                id = user.Id!,
                                name = user.Name!,
                                avatar = user.Avatar,
                                status = true,
                                messages = new List<message_chat>(),
                            };
                            ChatManager.AddUserToRoom(roomId, z);
                            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                            var Users = ChatManager.CurrentUsersInRoom(roomId);
                            await Clients.Groups(roomId).SendAsync("cur_users_in_room", Users);
                            await Clients.Group(roomId).SendAsync("user_in_room", z);
                            await Clients.All.SendAsync("cur_user_in_room", ChatManager.numBerUserInRoom());

                        }
                    }
                }
            }
        }
        public async Task userLeaveChatRoom(string userId, string roomId, string connectionId)
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
                var userInRoom = ChatManager.FindUserInRoom(roomId, user.UserId, connectionId);
                if (userInRoom != null)
                {
                    ChatManager.RemoveUserFromGroup(roomId, userInRoom);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                    await Clients.Groups(roomId).SendAsync("cur_users_leave_room", userInRoom);
                    await Clients.All.SendAsync("cur_user_in_room", ChatManager.numBerUserInRoom());
                }
            }
        }
        public async Task GetCurUserInRoom(string roomId)
        {
            var Users = ChatManager.CurrentUsersInRoom(roomId);
            await Clients.Groups(roomId).SendAsync("cur_users_in_room", Users);

        }
        public async Task UserChatToRoom(string connectionId,string userId, string roomId, string message)
        {
            var data = ChatManager.ChatToRoom(connectionId,userId, roomId, message);
            if(data != null) await Clients.Group(roomId).SendAsync("new_data_chat", data);
        }
        public async Task CloseAndSaveChat()
        {
            if (ChatManager.Rooms.TryGetValue(ChatManager.ListRoomName, out var listRoom))
            {
                foreach (var room in listRoom)
                {
                    for(int i=3; i>=0; i--) {
                        await Task.Delay(1000);
                        await Clients.Group(room.RoomId).SendAsync("close_room_chat",
                            $"Phòng chat của bạn sẽ đóng trong {i}");
                    }
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.RoomId);
                }
            }
        }
        public async Task GetDataChat(string roomId)
        {
            var data= ChatManager.DataChatRoom(roomId, 1,30);
            await Clients.User(Context.ConnectionId).SendAsync("data_room_chat", data);

        }
    }
    public class ChatManager
    {
        public static Dictionary<string, List<RoomChat>> Rooms = new Dictionary<string, List<RoomChat>>();
        public static Dictionary<string, List<user_chat>> UsersChat = new Dictionary<string, List<user_chat>>();
        private static readonly WebTruyenTranh_v2Context _db = new WebTruyenTranh_v2Context();
        public static string ListRoomName = "Rooms";
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
            if(!Rooms.ContainsKey(ListRoomName))
            {
                return false;
            }
            return Rooms[ListRoomName].Any(x => x.RoomId.Equals(roomId));

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
            else
            {
                if (!UsersChat[roomId].Any(x => x.id.Equals(user.id)) || !UsersChat[roomId].Any(x => x.id.Equals(user.id) && x.status == true))
                {
                    UsersChat[roomId].Add(user);
                }
            }     
        }
        public static List<user_chat>? CurrentUsersInRoom(string roomId)
        {
            if(UsersChat.TryGetValue(roomId,out var ListUsers))
            {
                return ListUsers.Where(x=> x.status == true).ToList();
            }
            return null;
        }
        public static List<countUser_room> numBerUserInRoom()
        {
            List<countUser_room> List = new List<countUser_room>();
            foreach(var room in UsersChat)
            {
                countUser_room a = new countUser_room
                {
                    roomId = room.Key,
                    numberuser =room.Value.Where(x=> x.status == true).Count()  /*UsersChat.GetValueOrDefault(room.Key, new List<user_chat>()).Count*/
                };
                List.Add(a);
            }
            return List;
        }
        public static int CheckUserExitsInAnyRoom(string userId, string connectionId)
        {
            if (UsersChat.Values.Any())
            {
                var check = UsersChat.Values
                    .Where(x => x.Any(y => y.id.Equals(userId) && y.status == true && y.conId != connectionId)).Count();
                return check;
            }
            return 0;
           
        }
        public static List<string>? FindUserInAnyRoom(string connetionId, string userId)
        {
            var roomId= UsersChat
                .Where(x=> 
                x.Value.Any(y=> 
                y.id.Equals(userId) && y.conId.Equals(connetionId) && y.status == true)).Select(z=> z.Key).ToList();
            return roomId;
        }
        ////xóa user khỏi một group {chuyển room, nếu dùng connectionid để xóa thì có nghĩa một uer có thể dùng nhiều thiết bị vào cùng một phòng}
        //Thay đổi quan trọng {chuyển từ xóa sang status=false}
        public static void RemoveUserFromGroup(string roomId, user_chat user)
        {
            var userin= UsersChat[roomId].FirstOrDefault(x=> x.Equals(user) && x.status == true);
            if(userin != null)
            {
                userin.status = false;
            }
            //if (UsersChat.ContainsKey(roomId) && UsersChat[roomId] != null)
            //{
            //    user_chat? user = UsersChat[roomId].SingleOrDefault(x => x.id.Equals(userId));
            //    if (user != null)
            //    {
            //        user.status = false;
            //        //UsersChat[roomId].Remove(user);
            //    }
            //    //Xóa group khỏi dictionary khi không còn user nào trong đó
            //    //if (UsersChat[roomId].Count == 0)
            //    //{
            //    //    UsersChat.Remove(roomId);
            //    //}
            //}
        }
        public static user_chat? FindUserInRoom(string roomId, string userId, string connectionId)
        {
            if(UsersChat.TryGetValue(roomId, out var litRoom))
            {
                return UsersChat[roomId]
                    .SingleOrDefault(x => x.id.Equals(userId) && x.status == true && x.conId.Equals(connectionId));
            }
            return null;
        }
        public static user_chat_view? ChatToRoom(string connectionId, string userId, string roomId, string message)
        {
            var user= UsersChat[roomId].SingleOrDefault(x => x.id.Equals(userId) && x.conId.Equals(connectionId) && x.status == true);
            if(user != null)
            {
                message_chat data = new message_chat
                {
                    data_chat = message,
                    time_chat = $"{DateTimeOffset.Now}",
                };
                user!.messages!.Add(data);
                user_chat_view b = new user_chat_view
                {
                    id = user.id,
                    name = user.name,
                    avatar = user.avatar,
                    messages = user.messages.OrderByDescending(x => x.time_chat).Select(y => y.data_chat).FirstOrDefault(),
                    time = user.messages.OrderByDescending(x => x.time_chat).Select(y => y.time_chat).FirstOrDefault()
                };
                return b;
            }
            return null;
           
        }
        public static IEnumerable<user_chat>? DataChatRoom(string roomId, int size ,int number)
        {
            if(UsersChat.TryGetValue(roomId, out var datachatroom))
            {
                int pagesize = (number -1) * size;
                int pagenumber = size;
                var data = datachatroom
                    .OrderByDescending(x=> x.messages!
                    .OrderByDescending(x=>x.time_chat)
                    .FirstOrDefault()?.time_chat)
                    .Skip(pagesize)
                    .Take(pagenumber);
                return data;
            }
            return null;
        }

        public static async Task SaveChatToDatabase()
        {
            foreach( var room in UsersChat)
            {
                string roomId = room.Key;
                foreach(var data in room.Value)
                {
                    var target = await _db.UserJoinChats.SingleOrDefaultAsync(x => x.RoomId.Equals(roomId) && x.UserId.Equals(data.id));
                    if(target != null)
                    {
                        if (data.messages != null)
                        {
                            for (int i = 0; i < data.messages.Count; i++)
                            {
                                Datachat a = new Datachat
                                {
                                    RoomId = roomId,
                                    UserId = data.id,
                                    Message = data.messages[i].data_chat,
                                    TimeChat = DateTimeOffset.Parse(data.messages[i].time_chat),
                                };
                                target!.Datachats.Add(a);
                            }
                            target.Status = false;
                            await _db.SaveChangesAsync();
                        }
                    }
                    data.messages = new List<message_chat>();
                }
            }
            //UsersChat.Clear();
        }
        public static async Task AuToCloseChat()
        {
            if (Rooms.ContainsKey(ListRoomName))
            {
                foreach(var room in Rooms[ListRoomName])
                {
                    var roomDB = await _db.ChatRooms.SingleOrDefaultAsync(x => x.RoomId.Equals(room.RoomId));
                    if(roomDB != null)
                    {
                        roomDB.EndTime = DateTimeOffset.UtcNow;
                        roomDB.Status = false;
                        await _db.SaveChangesAsync();
                    }
                }
                Rooms[ListRoomName] = new List<RoomChat>();
                UsersChat.Clear();
            }
        }
      
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
        public string conId { get; set; }= null!;
        public string id { get; set; } = null!;
        public string name { get; set; } = null!;
        public string? avatar { get; set; }
        public bool status { get; set; }
        public List<message_chat>? messages { get; set; }
    }
    public class user_chat_view
    {
        public string id { get; set; } = null!;
        public string name { get; set; } = null!;
        public string? avatar { get; set; }
        public string? messages { get; set; }
        public string? time { get; set; }
    }
    public class message_chat
    {
        public string? data_chat { get; set; }
        public string? time_chat { get; set; }
    }
    public class countUser_room
    {
        public string roomId { get; set; } = null!;
        public int numberuser { get; set; }
    }
}
