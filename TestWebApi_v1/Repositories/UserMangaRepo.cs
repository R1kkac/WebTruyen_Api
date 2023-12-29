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
using TestWebApi_v1.Models.ViewModel.UserView;
using TestWebApi_v1.Service.Respone;
using X.PagedList;

namespace TestWebApi_v1.Repositories
{
    public class UserMangaRepo:IUserMangaRepo
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly WebTruyenTranh_v2Context _db;
        private readonly IMapper _mapper;
        private readonly IServiceRepo _sv;
        public UserMangaRepo(UserManager<User> userManager, WebTruyenTranh_v2Context db, IMapper mapper, RoleManager<Role> roleManager,
            IServiceRepo sv)
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
            var map= _mapper.Map<List<ResponeMangaInfo>>(dsTruyen);
            map.ForEach(x =>
            {
                x.requesturl = requestUrl;
                x.routecontroller = routeController;
            });
            var map2 = _mapper.Map<List<MangaFollowing>>(map);
            return map2;
        }
        public async Task<ResponeData> TheoDoiTruyen(string IdUser, string IdManga)
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
                return new ResponeData { Value = true, Message = "Successfully" };
            }
            catch (Exception)
            {
                return new ResponeData { Value = false, Message = "Failed!" };
            }
        }
        public ResponeData HuyTheoDoi(string IdUser, string IdManga)
        {
            try
            {
                var bookmark =_db.bookmark.FirstOrDefault(x => x.IdUser.Equals(IdUser) && x.IdBotruyen.Equals(IdManga));
                if (bookmark != null)
                {
                    _db.bookmark.Remove(bookmark);
                    _db.SaveChanges();
                    return new ResponeData { Value = true, Message = "Successfully!" };
                }
                return new ResponeData { Value = false, Message = "Failed!" };
            }
            catch (Exception)
            {
                return new ResponeData { Value = false, Message = "Error!" };
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
        public async Task<IEnumerable<ResponeNotification>> LayThongBaoChuaDoc(string idUser, string requestUrl)
        {
            var result =await _db.Notification.Where(x => x.IdUser.Equals(idUser) && x.seen == false).ToListAsync();
            var data = _mapper.Map<List<ResponeNotification>>(result);
            foreach (var item in data) {
                var manga =await _db.BoTruyens.FindAsync(item.target);
                if(manga != null)
                {
                    item.nametarget = manga.MangaName;
                    item.imagerarget = $"{requestUrl}Truyen-tranh/{manga.Id}/{manga.MangaImage}";
                }
            }
            if(data != null)
            {
                return data;
            }
            return new List<ResponeNotification>() { };
        }
        //Tạo thông báo {admin=> user, user=> user (tag chat), phần thông báo chương mới truyện theo dõi sẽ được làm bằng trigger}
        public async Task<ResponeData> TaoThongBao(string idUser, string mesage)
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
                return new ResponeData { Value = true, Message = "Successfully!" };
            }
            return new ResponeData { Value = false, Message = "Failed!" };
        }
        /*Lưu ý: Phần tự xóa thông báo sẽ được thực hiện tự động bằng CSDL thông qua Stores Procedures và Sql Agent jobs,
         Thông báo không có phần tự động cập nhập, thông báo về chương truyện mới trong danh sách theo dõi được tạo bằng trigger trong
        bảng chuongtruyen*/
        public async Task<ResponeData> DaXemThongBao(string idThongbao)
        {
            try
            {
                var thongbao = await _db.Notification.FindAsync(idThongbao);
                thongbao!.seen = true;
                _db.SaveChanges();
                return new ResponeData { Value = true, Message = "Successfully" };
            }
            catch (Exception)
            {
                return new ResponeData { Value = false, Message = "Failed!" };
            }
        }

        public async Task<IEnumerable<ResponeManga>> DanhSachTruyenUserTao(string idUser)
        {
            var ds = _mapper.Map<List<ResponeManga>>(await _db.BoTruyens.Where(x=>x.Id==idUser).ToListAsync());
            if(ds!=null)
            {
                return ds;
            }
            return new List<ResponeManga>() { };
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


        public async Task DemViewBoTruyen(string idBotruyen)
        {
            await Task.Run(() =>
            {
                MangaviewCount.taoIndexView(idBotruyen);
                MangaviewCount.themViewBotruyen(idBotruyen);
            });
        }
        //Danh sách bình luận theo chương truyện
        public async Task<List<danhSachBinhLuan>> danhSachBinhLuanTheoChuuong(string mangaId,string idChuong, string requesturl)
        {
            //var data= (from chuongtruyen in _db.ChuongTruyens 
            //            join binhluan in _db.BinhLuans on chuongtruyen.ChapterId equals binhluan.ChapterId
            //            join user in _db.Users on binhluan.IdUser equals user.Id
            //           where chuongtruyen.ChapterId == idChuong select user).FirstOrDefaultAsync();
            var data = new List<danhSachBinhLuan>();
            var dsBinhluan= await _db.BinhLuans.Where(x=> x.MangaId.Equals(mangaId) && x.ChapterId== idChuong).ToListAsync() ?? null;
            if (dsBinhluan != null)
            {
                foreach (var item in dsBinhluan)
                {
                    var user =await _userManager.FindByIdAsync(item.IdUser);
                    danhSachBinhLuan a = new danhSachBinhLuan
                    {
                        UserName= user.Name!,
                        ChapterId = item.ChapterId ?? null,
                        MangaId= item.MangaId,
                        commentData = item.CommentData,
                        Likecomment = item.Likecomment,
                        Dislikecomment = item.Dislikecomment,
                        date = (DateTimeOffset)item.DateComment!,
                        IdComment = item.IdComment,
                        IdUser = user.Id,
                        UserAvatar = $"{requesturl}Authentication/Avatar/{user.Avatar}"
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
                var rootusercomment =await _db.Users.Where(x => x.BinhLuans.Any(y => y.IdComment.Equals(idComment))).SingleOrDefaultAsync();
                var result = await (from user in context.Users
                                    join data in context.ReplyComments on user.Id equals data.IdUserReply
                                    where data.IdComment == idComment
                                    select new danhSachReplyBinhLuan
                                    {
                                        IdUser = user.Id!,
                                        UserName = user.Name!,
                                        UserAvatar = $"https://localhost:7132/Authentication/Avatar/{user.Avatar}",
                                        IdReply = data.IdReply,
                                        NameReply= rootusercomment!.Name ?? null,
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Idchapter">id chương</param>
        /// <param name="page">kích thước trang</param>
        /// <param name="number">trang cần trỏ</param>
        /// <param name="requesturl">url của api</param>
        /// <returns>mapper của bình luận CommentViewModel</returns>
        public async Task<List<ResponeComment>> danhSachBinhluanCuaBoTruyen(string IdManga, int? page, int? number, string requesturl)
        {
            int pagesize = (page ?? 10);
            int pagenumber = (number ?? 1);
            try
            {
                var result = await _db.BinhLuans.Where(x=> x.MangaId.Equals(IdManga))
                    .OrderByDescending(x => x.DateComment)
                    .AsNoTracking()
                    .ToPagedListAsync(pagenumber, pagesize);
                List<ResponeComment> data = _mapper.Map<List<ResponeComment>>(result);

                foreach (var item in data)
                {
                    var user = await _db.Users.Where(y => y.Id.Equals(item.IdUser)).SingleOrDefaultAsync();
                    item.Avatar = $"{requesturl}Authentication/Avatar/{user!.Avatar}";
                    item.Name = user.Name;
                    item.CurChapter = await _db.ChuongTruyens
                    .Where(y => y.ChapterId.Equals(item.ChapterId))
                    .Select(z => z.ChapterName)
                    .SingleOrDefaultAsync();
                }
                return data;
            }
            catch (Exception)
            {
                return new List<ResponeComment>() { };
            }
        }
        //bình luận
        public async Task<bool> BinhLuanChuongTruyen(string IdUser,string IdManga, string? IdChapter, string CommentData)
        {
            if (await _userManager.FindByIdAsync(IdUser) == null || await _db.BoTruyens.FindAsync(IdManga) == null) return false;
            BinhLuan comment = new BinhLuan
            {
                IdComment = Guid.NewGuid().ToString().Substring(0, 10),
                IdUser = IdUser,
                MangaId= IdManga,
                ChapterId = (IdChapter == "isnull")? null : IdChapter,
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
        public async Task<bool> DanhgiaTruyen(string MangaId, string star)
        {
            try {
                var MangaRating = await _db.RatingMangas.Where(x => x.Mangaid == MangaId).FirstOrDefaultAsync();
                if (MangaRating != null)
                {
                    double rating = MangaRating.Rating;
                    int NumberUserRating = MangaRating.NumberRating;
                    double curRating = double.Parse(star);
                    double newRating = ((curRating) + (rating * NumberUserRating)) / (NumberUserRating + 1);
                    MangaRating.Rating = Math.Round(newRating, 2);
                    MangaRating.NumberRating += 1;
                    await _db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    double curRating = double.Parse(star);
                    RatingManga rate = new RatingManga
                    {
                        Mangaid = MangaId,
                        Rating = Math.Round(curRating, 2),
                        NumberRating = 1
                    };
                    await _db.RatingMangas.AddAsync(rate);
                    await _db.SaveChangesAsync();
                    return true;
                }
            }catch(Exception)
            {
                return false;
            }
        }
        public async Task<bool> likeComment(string Idcomment)
        {
            var result =await _db.BinhLuans.Where(x => x.IdComment.Equals(Idcomment)).SingleOrDefaultAsync();
            if(result != null)
            {
                result.Likecomment = result.Likecomment ?? 0;
                result.Likecomment += 1;
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> disLikeComment(string Idcomment)
        {
            var result = await _db.BinhLuans.Where(x => x.IdComment.Equals(Idcomment)).SingleOrDefaultAsync();
            if (result != null)
            {
                result.Dislikecomment = result.Dislikecomment ?? 0;
                result.Dislikecomment += 1;
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> unlikeComment(string Idcomment)
        {
            var result = await _db.BinhLuans.Where(x => x.IdComment.Equals(Idcomment)).SingleOrDefaultAsync();
            if (result != null)
            {
                result.Likecomment = result.Likecomment ?? 0;
                if(result.Likecomment > 0)
                {
                    result.Likecomment -= 1;
                }
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<bool> undisLikeComment(string Idcomment)
        {
            var result = await _db.BinhLuans.Where(x => x.IdComment.Equals(Idcomment)).SingleOrDefaultAsync();
            if (result != null)
            {
                result.Dislikecomment = result.Dislikecomment ?? 0;
                if(result.Dislikecomment> 0)
                {
                    result.Dislikecomment -= 1;
                }
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<int> numberComment(string mangaId)
        {
            var result = await _db.BinhLuans.Where(x => x.MangaId.Equals(mangaId)).CountAsync();
            return result;
        }
        private string RandomNumber()
        {
            Guid data = Guid.NewGuid();
            return data.ToString();
        }
    }
    public class MangaviewCount
    {
        public static Dictionary<string, int> ViewBotruyen = new Dictionary<string, int>();
        private static readonly object lockObject = new object();
        private static readonly WebTruyenTranh_v2Context _db = new WebTruyenTranh_v2Context();
        public static bool taoIndexView(string mangaid)
        {
            lock (lockObject)
            {
                if (string.IsNullOrEmpty(mangaid))
                {
                    // Đảm bảo mangaid không rỗng hoặc null
                    return false;
                }
                if (ViewBotruyen.ContainsKey(mangaid))
                {
                    return true;
                }
                else
                {
                    ViewBotruyen.Add(mangaid, 0);
                    return true;
                }
            }
        }
        public static bool themViewBotruyen(string mangaid)
        {
            lock (lockObject)
            {
                if (string.IsNullOrEmpty(mangaid))
                {
                    // Đảm bảo mangaid không rỗng hoặc null
                    return false;
                }
                if (ViewBotruyen.ContainsKey(mangaid))
                {
                    ViewBotruyen[mangaid] = ViewBotruyen[mangaid] + 1;
                    return true;
                }
                return false;
            }
        }
        public static async Task PushViewToDatabase()
        {
            if (ViewBotruyen.Count == 0)
            {
                return;
            }

            for (int i = 0; i < ViewBotruyen.Count; i++)
            {
                var mangaId = ViewBotruyen.Keys.ElementAt(i);
                var viewCount = ViewBotruyen[mangaId];

                var view = await _db.ViewCounts.FirstOrDefaultAsync(x => x.Id == mangaId);

                if (view != null)
                {
                    view.Viewbydate += viewCount;
                    view.Viewbymonth += view.Viewbydate;
                    view.Viewbyyear += view.Viewbymonth;
                }
            }

            await _db.SaveChangesAsync();
            ViewBotruyen.Clear();
        }
        public static void reMoveAllViewCount()
        {
            lock (lockObject)
            {
                if (ViewBotruyen.Count > 0)
                {
                    ViewBotruyen.Clear();
                }
            }    
        }
    }
}
