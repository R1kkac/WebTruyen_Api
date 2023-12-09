using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TestWebApi_v1.Controllers;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel.MangaView;
using TestWebApi_v1.Service;
using X.PagedList;

namespace TestWebApi_v1.Repositories
{
    public class User_Manga_Model:IUser_Manga_Model
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly WebTruyenTranh_v2Context _db;
        private readonly IMapper _mapper;
        private readonly IServices _sv;
        public User_Manga_Model(UserManager<User> userManager, WebTruyenTranh_v2Context db, IMapper mapper, RoleManager<Role> roleManager,
            IServices sv)
        {
            _userManager = userManager;
            _db = db;
            _mapper = mapper;
            _roleManager = roleManager;
            _sv = sv;
        }

        //Theo doi truyen
        //public async Task<List<Bookmark>> DanhSachTheoDoi(string idUser)
        //{
        //    var danhsach =await _db.bookmark.Where(x=>x.IdUser.Equals(idUser)).ToListAsync();
        //    if (danhsach != null)
        //    {
        //        return danhsach;
        //    }
        //    return new List<Bookmark> { };
        //}
        public async Task<List<MangaFollowing>> DanhSachTheoDoi(string idUser, string requestUrl)
        {
            string routeController= "Truyen-tranh";
            var dsTruyen = await (from Bookmark in _db.bookmark
                           join BoTruyen in _db.BoTruyens on Bookmark.IdBotruyen equals BoTruyen.MangaId
                           where Bookmark.IdUser == idUser
                           select BoTruyen).ToListAsync();
            var map= _mapper.Map<List<BotruyenProfile>>(dsTruyen);
            map.ForEach(x =>
            {
                x.requesturl = requestUrl;
                x.routecontroller = routeController;
            });
            var map2 = _mapper.Map<List<MangaFollowing>>(map);
            return map2;
        }
        public async Task<ResultService> TheoDoiTruyen(string IdUser, string IdManga)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(IdUser);
                var bookmark = new Bookmark
                {
                    IdUser = user.Id,
                    IdBotruyen = IdManga
                };
                user.bookmarks.Add(bookmark);
                _db.SaveChanges();
                return new ResultService { Value = true, Message = "Successfully" };
            }
            catch (Exception)
            {
                return new ResultService { Value = false, Message = "Failed!" };
            }
        }
        public ResultService HuyTheoDoi(string IdUser, string IdManga)
        {
            try
            {
                var bookmark =_db.bookmark.FirstOrDefault(x => x.IdUser.Equals(IdUser) && x.IdBotruyen.Equals(IdManga));
                if (bookmark != null)
                {
                    _db.bookmark.Remove(bookmark);
                    _db.SaveChanges();
                    return new ResultService { Value = true, Message = "Successfully!" };
                }
                return new ResultService { Value = false, Message = "Failed!" };
            }
            catch (Exception)
            {
                return new ResultService { Value = false, Message = "Error!" };
            }
        }

        //Thong bao
        public async Task<IEnumerable<ThongbaoUser>> LayToanBoThongBao(string idUser)
        {
            var result =await _db.Notification.Where(x => x.IdUser.Equals(idUser)).ToListAsync();
            if (result != null)
            {
                return result;
            }
            return new List<ThongbaoUser>() { };
        }
        public async Task<IEnumerable<ThongbaoUser>> LayThongBaoChuaDoc(string idUser)
        {
            var result =await _db.Notification.Where(x => x.IdUser.Equals(idUser) && x.seen == false).ToListAsync();
            if(result != null)
            {
                return result;
            }
            return new List<ThongbaoUser>() { };
        }
        //Tạo thông báo {admin=> user, user=> user (tag chat), phần thông báo chương mới truyện theo dõi sẽ được làm bằng trigger}
        public async Task<ResultService> TaoThongBao(string idUser, string mesage)
        {
            var user =await _userManager.FindByIdAsync(idUser);
            if (user != null)
            {
                var ListthongBao = await _db.Notification.Select(x => x.Id).ToListAsync();
                var idThongBao = RandomNumber();
                while (ListthongBao.Contains(idThongBao.ToString()))
                {
                    idThongBao= RandomNumber();
                }
                var thongBao = new ThongbaoUser
                {
                    Id = idThongBao,
                    dateTime= DateTime.UtcNow,
                    IdUser = idUser,
                    message=mesage,
                    seen=false,
                    IdNavigation=user
                };
                user.Notification.Add(thongBao);
                _db.SaveChanges();
                return new ResultService { Value = true, Message = "Successfully!" };
            }
            return new ResultService { Value = false, Message = "Failed!" };
        }
        /*Lưu ý: Phần tự xóa thông báo sẽ được thực hiện tự động bằng CSDL thông qua Stores Procedures và Sql Agent jobs,
         Thông báo không có phần tự động cập nhập, thông báo về chương truyện mới trong danh sách theo dõi được tạo bằng trigger trong
        bảng chuongtruyen*/
        public async Task<ResultService> DaXemThongBao(string idThongbao)
        {
            try
            {
                var thongbao = await _db.Notification.FindAsync(idThongbao);
                thongbao!.seen = true;
                _db.SaveChanges();
                return new ResultService { Value = true, Message = "Successfully" };
            }
            catch (Exception)
            {
                return new ResultService { Value = false, Message = "Failed!" };
            }
        }

        public async Task<IEnumerable<botruyenView>> DanhSachTruyenUserTao(string idUser)
        {
            var ds = _mapper.Map<List<botruyenView>>(await _db.BoTruyens.Where(x=>x.Id==idUser).ToListAsync());
            if(ds!=null)
            {
                return ds;
            }
            return new List<botruyenView>() { };
        }
        //Danh sách user theo role với từ khóa là IdRole
        public async Task<IEnumerable<User>> DanhSachUserTheoRole(string idRole)
        {
            var listUser = from UserRoles in _db.UserRoles join User in _db.Users on UserRoles.UserId equals User.Id
                           where UserRoles.RoleId==idRole select User;
            var userList = await listUser.ToListAsync();
            return userList;
        }
        //Danh sách user theo role với từ khóa là RoleName
        public async Task<IEnumerable<User>> DanhSacUserRoleName(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            var listUser = from UserRoles in _db.UserRoles
                           join User in _db.Users on UserRoles.UserId equals User.Id
                           where UserRoles.RoleId == role.Id
                           select User;
            var userList = await listUser.ToListAsync();
            return userList;
        }


        public async Task<ResultService> DemViewBoTruyen(string idBotruyen, string View)
        {
            var viewCount =await _db.ViewCounts.FirstOrDefaultAsync(x => x.Id.Equals(idBotruyen));
            if (viewCount != null)
            {
                viewCount.Viewbydate += int.Parse(View);
                await _db.SaveChangesAsync();
                return new ResultService { Value = true, Message = "Successfully!" };
            }
            return new ResultService { Value = false, Message = "Failed!" };
        }
        //Danh sách bình luận theo chương truyện
        public async Task<List<danhSachBinhLuan>> danhSachBinhLuanTheoChuuong(string idChuong)
        {
            //var data= (from chuongtruyen in _db.ChuongTruyens 
            //            join binhluan in _db.BinhLuans on chuongtruyen.ChapterId equals binhluan.ChapterId
            //            join user in _db.Users on binhluan.IdUser equals user.Id
            //           where chuongtruyen.ChapterId == idChuong select user).FirstOrDefaultAsync();
            var data = new List<danhSachBinhLuan>();
            var dsBinhluan= await _db.BinhLuans.Where(x=>x.ChapterId== idChuong).ToListAsync() ?? null;
            if (dsBinhluan != null)
            {
                foreach (var item in dsBinhluan)
                {
                    var user =await _userManager.FindByIdAsync(item.IdUser);
                    danhSachBinhLuan a = new danhSachBinhLuan
                    {
                        UserName= user.Name!,
                        chapterId = item.ChapterId,
                        commentData = item.CommentData,
                        date = (DateTimeOffset)item.DateComment!,
                        IdComment = item.IdComment,
                        IdUser = user.Id,
                        UserAvatar = $"https://localhost:7132/Authentication/Avatar/{user.Avatar}"
                    };
                    data.Add(a);
                }
            }
            if (dsBinhluan == null)
            {
                return new List<danhSachBinhLuan>() { };
            }
            return data;
        }
        //Danh sách phản hồi bình luận
        public async Task<List<danhSachReplyBinhLuan>> layDanhSachPhanHoi(string idComment)
        {
            //dùng using thế này để tạo kết nối ngắn hạn tới database và nó sẽ kết thúc kết nối khi kết thúc using tránh dữ liệu
            // không đồng bộ khi vẫn còn giữ kết nối nhưng dữ liệu database thay đổi
            using (var context= new WebTruyenTranh_v2Context())
            {
                var result = await (from user in context.Users
                                    join data in context.ReplyComments on user.Id equals data.IdUserReply
                                    where data.IdComment == idComment
                                    select new danhSachReplyBinhLuan
                                    {
                                        IdUser = user.Id!,
                                        UserName = user.UserName!,
                                        UserAvatar = $"https://localhost:7132/Authentication/Avatar/{user.Avatar}",
                                        IdReply = data.IdReply,
                                        ReplyData = data.Replydata,
                                        DateReply = data.DateReply
                                    }
                ).ToListAsync();
                context.Dispose();
                return result;
            }
            //var result =await (from user in _db.Users
            //              join data in _db.ReplyComments on user.Id equals data.IdUserReply
            //              where data.IdComment == idComment
            //              select new danhSachReplyBinhLuan
            //              {
            //                  IdUser = user.Id!,
            //                  UserName = user.UserName!,
            //                  UserAvatar= $"https://localhost:7132/Authentication/Avatar/{user.Avatar}",
            //                  IdReply = data.IdReply,
            //                  ReplyData= data.Replydata,
            //                  DateReply= data.DateReply
            //              }
            //    ).ToListAsync();
            //return result;
        }
        //bình luận
        public async Task<bool> BinhLuanChuongTruyen(string IdUser, string IdChapter, string CommentData)
        {
            if (await _userManager.FindByIdAsync(IdUser) == null || await _db.ChuongTruyens.FindAsync(IdChapter) == null) return false;
            BinhLuan comment = new BinhLuan
            {
                IdComment = Guid.NewGuid().ToString().Substring(0, 10),
                IdUser = IdUser,
                ChapterId = IdChapter,
                CommentData = CommentData,
                DateComment = DateTime.UtcNow
            };
            try
            {
                await _db.BinhLuans.AddAsync(comment);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (SqlException)
            {
                return false;

            }         
        }
        //Reply bình luận
        public async Task<bool> ReplyBinhLuanChuong(string IdComment, string IdUser, string Replydata)
        {
            if(await _db.BinhLuans.FindAsync(IdComment) != null && await _userManager.FindByIdAsync(IdUser) != null)
            {
                ReplyComment reply = new ReplyComment
                {
                    IdReply = Guid.NewGuid().ToString().Substring(0, 10),
                    IdComment = IdComment,
                    IdUserReply = IdUser,
                    Replydata = Replydata,
                    DateReply = DateTime.UtcNow
                };
                try
                {
                    await _db.ReplyComments.AddAsync(reply);
                    await _db.SaveChangesAsync();
                    return true;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
            return false;
        }
        private string RandomNumber()
        {
            Guid data = Guid.NewGuid();
            return data.ToString();
        }

       
    }
}
