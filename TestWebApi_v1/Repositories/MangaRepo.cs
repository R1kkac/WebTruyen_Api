using AutoMapper;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel;
using TestWebApi_v1.Models.ViewModel.MangaView;
using Twilio.Base;
using X.PagedList;
using static StackExchange.Redis.Role;
using TestWebApi_v1.Models.ResponeViewModel.ResponeManga;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TestWebApi_v1.Repositories
{
    public class MangaRepo:IMangaRepo
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IWebHostEnvironment _env;
        private WebTruyenTranh_v2Context _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IServiceRepo _sv;
        private static string data = "Manga";
        private readonly IDistributedCache _cache;
        public MangaRepo(IWebHostEnvironment webHostEnvironment,WebTruyenTranh_v2Context db, UserManager<User> user,
            RoleManager<Role> role, IMapper mapper, IServiceRepo sv,IMemoryCache memoryCache,IDistributedCache cache)
        {
            _db = db;
            _env = webHostEnvironment;
            _userManager = user;
            _roleManager = role;
            _mapper = mapper;
            _sv = sv;
            _memoryCache= memoryCache;
            _cache = cache;
        }
        //------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------Manga-----------------------------------------------------//
        //Dữ liệu search
        public async Task<IEnumerable<Searchmanga>> search()
        {
            Stopwatch t = new Stopwatch();
            t.Start();
            var result = await _db.BoTruyens.ToListAsync();
            var data = _mapper.Map<List<Searchmanga>>(result);
            t.Stop();
            Console.WriteLine("Elasped Time:" + t.Elapsed);
            if (data != null)
                return data;
            return new List<Searchmanga> { };
        }
        //Search thời gian thực
        public async Task<IEnumerable<Searchmanga>> responeSearch(string value, string url)
        {
            var result=await _db.BoTruyens.Where(x=>x.MangaName.Contains(value)).ToListAsync();
            foreach(var x in result)
            {
                x.MangaImage = $"{url}/{x.MangaId}/{x.MangaImage}";
            }
            var data = _mapper.Map<List<Searchmanga>>(result);
            return data;
        }
		//Lấy tất cả truyện
		public async Task<IEnumerable<CRUDView>> LayTatCaTruyen(string requestUrl, string routeController)
		{
			// Sử dụng bộ nhớ cục bộ để caching
			if (_memoryCache.TryGetValue("ListManga", out List<CRUDView> cachedList))
			{
				return cachedList;
			}

			// Truy vấn cơ sở dữ liệu để lấy toàn bộ danh sách truyện
			var dsbotruyen = await _db.BoTruyens.AsNoTracking()
								.OrderByDescending(x => x.Dateupdate)
								.ToListAsync();

			var result = new List<CRUDView>();

			foreach (var a in dsbotruyen)
			{
				RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
				var map1 = _mapper.Map<ResponeMangaInfo>(a);
				map1.requesturl = requestUrl;
				map1.routecontroller = routeController;
				var mapmanga = _mapper.Map<CRUDView>(map1);
				mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
				List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).OrderByDescending(y => y.ChapterDate)
					.Take(3).ToListAsync();
				var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
				var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbydate).FirstOrDefaultAsync();
				var mapchapter = _mapper.Map<List<chapterView2>>(listChapter);
				mapmanga.ListChaper = mapchapter;
				mapmanga.Listcategory = listCategory;
				mapmanga.View = mangaview.ToString();
				result.Add(mapmanga);
			}

			return result;
		}
		//Gọi danh sác truyện theo page với 3 tham số currentUrl, route, page
		public async Task<IEnumerable<ResponeManga>> LayDanhSachTruyenTheoPage(int? page, string requestUrl,string routeController)
        {
            //bộ nhớ cục bộ
            if (_memoryCache.TryGetValue("ListManga", out List<ResponeManga> listdata))
            {
                var result = new List<ResponeManga>();
                foreach (var a in listdata)
                {
                    RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                    var map1 = _mapper.Map<ResponeMangaInfo>(a);
                    map1.requesturl = requestUrl;
                    map1.routecontroller = routeController;
                    var mapmanga = _mapper.Map<ResponeManga>(map1);
                    mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                    List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).OrderByDescending(y => y.ChapterDate)
                        .Take(3).ToListAsync();
                    var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
                    var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbyyear).FirstOrDefaultAsync();
                    var mapchapter = _mapper.Map<List<chapterView2>>(listChapter);
                    mapmanga.ListChaper = mapchapter;
                    mapmanga.Listcategory = listCategory;
                    mapmanga.View = mangaview.ToString();
                    result.Add(mapmanga);
                }     
                return result;
            }
            else
            {
                /*bộ nhớ phân táng IDistributedCache
                var aad = _cache.GetString("ListManga");*/
                int pageSize = 10; // Số lượng bản ghi trên mỗi trang
                int pageNumber = (page ?? 1); // Trang hiện tại
                var dsbotruyen = await _db.BoTruyens.Where(x=> x.DeleteStatus == true).AsNoTracking().OrderByDescending(x => x.Dateupdate).ToPagedListAsync(pageNumber, pageSize);
                var result = new List<ResponeManga>();
                foreach (var a in dsbotruyen)
                {
                    RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                    var map1 = _mapper.Map<ResponeMangaInfo>(a);
                    map1.requesturl = requestUrl;
                    map1.routecontroller = routeController;
                    var mapmanga = _mapper.Map<ResponeManga>(map1);
                    mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                    List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).OrderByDescending(y => y.ChapterDate)
                        .Take(3).ToListAsync();
                    var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
                    var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbyyear).FirstOrDefaultAsync();
                    var mapchapter = _mapper.Map<List<chapterView2>>(listChapter);
                    mapmanga.ListChaper = mapchapter;
                    mapmanga.Listcategory = listCategory;
                    mapmanga.View = mangaview.ToString();
                    result.Add(mapmanga);
                }
                //test thêm cache
                //YourMethod(result);       
                return result;
            }
        }
		//Lấy truyện theo UserId
		public async Task<IEnumerable<CRUDView>> LayTatCaTruyenTheoUserId(string userId, string requestUrl, string routeController)
		{
			// Sử dụng bộ nhớ cục bộ để caching
			if (_memoryCache.TryGetValue("ListManga", out List<CRUDView> cachedList))
			{
				return cachedList;
			}

			var dsbotruyen = await _db.BoTruyens
									.AsNoTracking()
									.Where(b => b.Id == userId) // Lọc theo UserId
									.OrderByDescending(x => x.Dateupdate)
									.ToListAsync();

			var result = new List<CRUDView>();

			foreach (var a in dsbotruyen)
			{
				RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
				var map1 = _mapper.Map<ResponeMangaInfo>(a);
				map1.requesturl = requestUrl;
				map1.routecontroller = routeController;
				var mapmanga = _mapper.Map<CRUDView>(map1);
				mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
				List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).OrderByDescending(y => y.ChapterDate)
					.Take(3).ToListAsync();
				var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
				var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbydate).FirstOrDefaultAsync();
				var mapchapter = _mapper.Map<List<chapterView2>>(listChapter);
				mapmanga.ListChaper = mapchapter;
				mapmanga.Listcategory = listCategory;
				mapmanga.View = mangaview.ToString();
				result.Add(mapmanga);
			}

			return result;
		}

		//tạo cache
		//Test thôi
		public void YourMethod(List<ResponeManga> data)
        {
            // Lưu trữ dữ liệu vào cache
            _memoryCache.Set("Datacache", data, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) // Thời gian sống của cache, ví dụ 15 phút.
            });
        }
        //Chi tiết một bộ truyện cụ thể
        public async Task<ResponeManga?> LayThongTinTruyen(string MangaId, string requestUrl, string routeController)
        {
            //var a =await _db.BoTruyens.FindAsync(MangaId);
            var a = await _db.BoTruyens.Where(x => x.MangaId.Equals(MangaId) && x.DeleteStatus == true).SingleOrDefaultAsync();
            if (a != null)
            {
                RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                var map1 = _mapper.Map<ResponeMangaInfo>(a);
                map1.requesturl = requestUrl;
                map1.routecontroller = routeController;
                var mapmanga = _mapper.Map<ResponeManga>(map1);
                mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).OrderByDescending(y => y.ChapterDate)
                    .ToListAsync();
                var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
                var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbyyear).FirstOrDefaultAsync();
                var mapchapter = _mapper.Map<List<chapterView2>>(listChapter);
                int sum = 0;
                sum += await _db.BinhLuans.Where(y => y.MangaId.Equals(mapmanga.MangaId)).CountAsync();
                mapmanga.Comment = sum;
                mapmanga.ListChaper = mapchapter;
                mapmanga.Listcategory = listCategory;
                mapmanga.View = mangaview.ToString();

                return mapmanga;
            }else{
                return null;
            }
        }
        //Lấy ảnh bìa của bộ truyện
        public string? LayAnhTruyen(string imageManga)
        {
            // Đường dẫn đến thư mục chứa hình ảnh "Manga"
            string imagePath = Path.Combine(_env.ContentRootPath, "Manga", "Content", imageManga);

            // Kiểm tra xem hình ảnh có tồn tại không
            if (System.IO.File.Exists(imagePath))
            {
                return imagePath;
            }
            return null;
        }
		//Lấy tên truyện
		public async Task<IEnumerable<string>> LayDanhSachTenTruyen()
		{
			// Lấy tất cả bản ghi từ bảng BoTruyens mà không cần phân trang
			var dsbotruyen = await _db.BoTruyens.AsNoTracking().ToListAsync();

			// Tạo và trả về danh sách chứa chỉ tên truyện
			var danhSachTenTruyen = dsbotruyen.Select(a => a.MangaName).ToList();

			return danhSachTenTruyen;
		}
		//Tạo bộ truyện
		public async Task<bool> TaoTruyen(string idUser, AddeditView mangaDto, IFormFile? MangaImage)
		{
			var user = await _userManager.FindByIdAsync(idUser);
			if (mangaDto != null)
			{
				var random = new Random();
				var mangaid = "";
				for (int i = 0; i < 6; i++)
				{
					mangaid += random.Next(10).ToString(); // Tạo số ngẫu nhiên từ 0 đến 9 và thêm vào chuỗi mangaid
				}
				BoTruyen manga = new BoTruyen
				{
					MangaId = mangaid,
					MangaName = mangaDto.MangaName,
					MangaDetails = mangaDto.MangaDetails,
					MangaImage = MangaImage != null ? MangaImage.FileName : null,
					MangaAlternateName = mangaDto.MangaAlternateName,
					MangaAuthor = mangaDto.MangaAuthor,
					MangaArtist = mangaDto.MangaArtist,
					Type = mangaDto.Type,
					Id = user.Id,
					Dateupdate = DateTime.Now,
					Status = false,
					DeleteStatus = true,

				};
				if (mangaDto.GenreIds != null)
				{
					foreach (var genreId in mangaDto.GenreIds)
					{
						var genre = await _db.TheLoais.FindAsync(genreId); // Tìm kiếm TheLoai dựa trên GenreId
						if (genre != null)
						{
							manga.Genres.Add(genre); // Thêm TheLoai vào BoTruyen
						}
					}
				}
				string d = "Manga";
				string FolderPath = _sv.OnGetFolderPath(d);
				string mangaImagePath = Path.Combine(FolderPath, "Content");
				if (_sv.CreateFolder(mangaid, FolderPath) == true)
				{
					if (MangaImage != null) await _sv.UpLoadimage(MangaImage, mangaImagePath);
					user.BoTruyens.Add(manga);
					//_db.BoTruyens.Add(manga);
					await _db.SaveChangesAsync();
					return true;
				}
			}
			return false;
		}
		//Sửa thông tin của bộ truyện
		public async Task<bool> SuaTruyen(string idUser, AddeditView mangaDto, IFormFile? MangaImage)
		{
			using (var transaction = _db.Database.BeginTransaction())
			{
				try
				{
					var truyen = await _db.BoTruyens.Include(bt => bt.Genres).FirstOrDefaultAsync(bt => bt.MangaId == mangaDto.MangaId);
					var user = await _userManager.FindByIdAsync(idUser);
					var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
					if (truyen != null && (isAdmin || truyen.Id.Equals(idUser)))
					{
						string d = "Manga";
						string FolderPath = _sv.OnGetFolderPath(d);
						string mangaImagePath = Path.Combine(FolderPath, "Content");

						// Cập nhật thông tin truyện
						truyen.MangaName = mangaDto.MangaName ?? truyen.MangaName;
						truyen.MangaDetails = mangaDto.MangaDetails ?? truyen.MangaDetails;
						truyen.MangaImage = (MangaImage != null) ? MangaImage.FileName : truyen.MangaImage;
						truyen.MangaAlternateName = mangaDto.MangaAlternateName ?? truyen.MangaAlternateName;
						truyen.MangaAuthor = mangaDto.MangaAuthor ?? truyen.MangaAuthor;
						truyen.MangaArtist = mangaDto.MangaArtist ?? truyen.MangaArtist;
						truyen.Type = mangaDto.Type ?? truyen.Type;
						truyen.Dateupdate = DateTime.Now;

						// Cập nhật GenreIds
						truyen.Genres.Clear(); // Xóa các liên kết hiện tại
						foreach (var genreId in mangaDto.GenreIds)
						{
							var genre = await _db.TheLoais.FindAsync(genreId);
							if (genre != null)
							{
								truyen.Genres.Add(genre); // Thêm liên kết mới
							}
						}

						await _db.SaveChangesAsync();

						if (MangaImage != null)
						{
							await _sv.UpLoadimage(MangaImage, mangaImagePath);
						}
						transaction.Commit();
						return true;
					}
					else
					{
						transaction.Rollback();
						return false;
					}
				}
				catch (Exception)
				{
					transaction.Rollback();
					return false;
				}
			}
		}
		//Sửa trạng thái (Status)
		public async Task<bool> UpdateStatusAsync(string mangaId, string idUser)
		{
			using (var transaction = _db.Database.BeginTransaction())
			{
				try
				{
					var user = await _userManager.FindByIdAsync(idUser);
					var boTruyen = await _db.BoTruyens.FirstOrDefaultAsync(bt => bt.MangaId == mangaId);
					var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

					if (boTruyen != null && (isAdmin || boTruyen.Id.Equals(idUser)))
					{
						boTruyen.Status = !boTruyen.Status; // Đảo ngược trạng thái hiện tại
						await _db.SaveChangesAsync();
						transaction.Commit();
						return true;
					}
					else
					{
						return false;
					}
				}
				catch (Exception)
				{
					transaction.Rollback();
					return false;
				}
			}
		}
		//Đưa truyện vào mục "Đã xóa" (DeleteStatus)
		public async Task<bool> DeleteStatus(string mangaId, string idUser)
		{
			using (var transaction = _db.Database.BeginTransaction())
			{
				try
				{
					var user = await _userManager.FindByIdAsync(idUser);
					var boTruyen = await _db.BoTruyens.FirstOrDefaultAsync(bt => bt.MangaId == mangaId);
					var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

					if (boTruyen != null && (isAdmin || boTruyen.Id.Equals(idUser)))
					{
						// Nếu truyện đang active (true) và sẽ được chuyển thành inactive (false)
						if (boTruyen.DeleteStatus.HasValue && boTruyen.DeleteStatus.Value)
						{
							boTruyen.MarkedAsDeletedDate = DateTime.UtcNow; // Đánh dấu thời gian xóa
						}
						else
						{
							boTruyen.MarkedAsDeletedDate = null; // Khi truyện được kích hoạt lại
						}

						boTruyen.DeleteStatus = !boTruyen.DeleteStatus; // Đảo ngược trạng thái hiện tại
						await _db.SaveChangesAsync();
						transaction.Commit();
						return true;
					}
					else
					{
						return false;
					}
				}
				catch (Exception)
				{
					transaction.Rollback();
					return false;
				}
			}
		}
		//Xóa bộ truyện
		public async Task<bool> XoaTruyen(string iduUser, string MangaId)	
		{
			var user = await _userManager.FindByIdAsync(iduUser);
			var truyen = await _db.BoTruyens
                .Include(bt => bt.Genres)
				.Include(bt => bt.ChatRooms)
					.ThenInclude(bt => bt.UserJoinChats)
				.Include(bt => bt.BinhLuans)
					.ThenInclude(bl => bl.ReplyComments)
				.Include(bt => bt.RatingMangas)
				.Include(bt => bt.BotruyenViewCounts)
				.FirstOrDefaultAsync(bt => bt.MangaId == MangaId);
			var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
			if (truyen != null && (isAdmin || truyen.Id.Equals(iduUser)))
			{
				foreach (var chatroom in truyen.ChatRooms)
				{
					foreach (var userJoinChat in chatroom.UserJoinChats)
					{
						var dataChats = _db.Datachats.Where(dc => dc.RoomId == userJoinChat.RoomId && dc.UserId == userJoinChat.UserId);
						_db.Datachats.RemoveRange(dataChats);
					}
				}
				await _db.SaveChangesAsync();
				//Xóa liên kết user với Roomchat
				foreach (var chatroom in truyen.ChatRooms)
				{
					_db.UserJoinChats.RemoveRange(chatroom.UserJoinChats);
				}
				// Xử lý xóa ChatRooms
				_db.ChatRooms.RemoveRange(truyen.ChatRooms);

				// Xóa ReplyComments trước
				foreach (var binhLuan in truyen.BinhLuans)
				{
					_db.ReplyComments.RemoveRange(binhLuan.ReplyComments);
				}

				// Xử lý xóa BinhLuans
				_db.BinhLuans.RemoveRange(truyen.BinhLuans);

				// Xử lý xóa RatingMangas
				_db.RatingMangas.RemoveRange(truyen.RatingMangas);

                // Xử lý xóa BotruyenViewCounts
                _db.ViewCounts.RemoveRange(truyen.BotruyenViewCounts);

				// Xử lý xóa các liên kết từ TheLoai
				foreach (var theLoai in truyen.Genres.ToList())
				{
					theLoai.Mangas.Remove(truyen);
				}

				// Tiếp tục xử lý xóa các chương truyện và ảnh liên quan
				string mangaFile = Path.Combine(_sv.OnGetFolderPath(data), truyen.MangaId);
				if (mangaFile != null) _sv.DeleteFolder(mangaFile);
				var chuongTruyens = _db.ChuongTruyens.Where(x => x.MangaId == truyen.MangaId).ToList();
				foreach (var chuong in chuongTruyens)
				{
					var image = _db.ChapterImages.Where(x => x.ChapterId == chuong.ChapterId);
					_db.ChapterImages.RemoveRange(image);
				}
				_db.ChuongTruyens.RemoveRange(chuongTruyens);

				// Xóa BoTruyen
				user.BoTruyens.Remove(truyen);
				_db.BoTruyens.Remove(truyen);

				string mangaImagePath = Path.Combine(_sv.OnGetFolderPath(data), "Content");
				_sv.DeleteImage(mangaImagePath + "\\" + truyen.MangaImage);

				await _db.SaveChangesAsync();/*không thể dùng await _db.savechange() ở đây vì sẽ xung đột với{UserModel
                                          => await _userManager.DeleteAsync(user)}; section này chưa kết thúc section khác đã tạo*/
				return true;
			}
			else
			{
				return false;
			}
		}
		//Lấy truyện mới cập nhật
		public async Task<List<ResponeManga>> getMangaNewUpdate(string requestUrl, string routeController)
        {
            using( var _context= _db)
            {
                List<ResponeManga> data = new List<ResponeManga>();
                var result =await _context.BoTruyens.Where(x=> x.DeleteStatus == true).OrderByDescending(item=> item.Dateupdate).Take(6).ToListAsync();
                foreach (var item in result)
                {
                    RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", item.MangaId).FirstOrDefaultAsync();
                    var map1 = _mapper.Map<ResponeMangaInfo>(item);
                    map1.requesturl = requestUrl;
                    map1.routecontroller = routeController;
                    var map2 = _mapper.Map<ResponeManga>(map1);
                    map2.Rating = rating?.Rating.ToString() ?? "N/A";

                    data.Add(map2);
                }
                return data;
            }
        }
        //Lấy số trang truyện
        public async Task<int> getPageNumber()
        {
            try
            {
                var result = await _db.BoTruyens.CountAsync();
                return (result/10) + (result % 10 >0 ? 1:0);
            }
            catch(SqlException ex)
            {
                throw ex;
            }
        }
        //lấy top manga
        public async Task<ResultForTopView> getTopmanga(int page, int number, int type, string requestUrl)
        {
            List<botruyenViewforTopmanga> data = new List<botruyenViewforTopmanga>();
            if(type == 0)
            {
                var result = await _db.BoTruyens
                    .Where(x=> x.DeleteStatus == true)
                    .OrderByDescending(x => x.BotruyenViewCounts.Sum(y => y.Viewbyyear))
                    .ToPagedListAsync(page, number);
                var numberManga = _db.BoTruyens.ToList().Count();
                foreach (var a  in result)
                {
                    RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                    var map1 = _mapper.Map<ResponeMangaInfo>(a);
                    map1.requesturl = requestUrl;
                    map1.routecontroller = "Truyen-tranh";
                    var mapmanga = _mapper.Map<botruyenViewforTopmanga>(map1);
                    mapmanga.Rating = rating?.Rating.ToString() ?? "0";
                    var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbyyear).FirstOrDefaultAsync();
                    var chaptercount =await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).CountAsync();
                    mapmanga.View = mangaview.ToString();
                    mapmanga.chaptercount = chaptercount.ToString();
                    mapmanga.mangaCount = result.Count().ToString();
                    mapmanga.TypeManga = await _db.TypeMangas.Where(x => x.Id == a.Type).Select(y=> y.Name).FirstOrDefaultAsync();
                    var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
                    mapmanga.numberFollow =_db.Users.Count(x => x.bookmarks.Any(y => y.IdBotruyen == a.MangaId));
                    mapmanga.Listcategory = listCategory;
                    data.Add(mapmanga);
                }
                return new ResultForTopView() { numberManga= numberManga , listmanga= data};
            }
            else
            {
                var result = await _db.BoTruyens
                    .Where(x => x.Type == type)
                    .OrderByDescending(z => z.BotruyenViewCounts.Sum(d => d.Viewbyyear))
                    .ToPagedListAsync(page, number);
                var numberManga = _db.BoTruyens.Where(x=> x.Type == type).Count();
                foreach (var a in result)
                {
                    RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                    var map1 = _mapper.Map<ResponeMangaInfo>(a);
                    map1.requesturl = requestUrl;
                    map1.routecontroller = "Truyen-tranh";
                    var mapmanga = _mapper.Map<botruyenViewforTopmanga>(map1);
                    mapmanga.Rating = rating?.Rating.ToString() ?? "0";
                    var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbyyear).FirstOrDefaultAsync();
                    var chaptercount = await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).CountAsync();
                    mapmanga.View = mangaview.ToString();
                    mapmanga.TypeManga = await _db.TypeMangas.Where(x => x.Id == a.Type).Select(y => y.Name).FirstOrDefaultAsync();
                    mapmanga.chaptercount = chaptercount.ToString();
                    mapmanga.mangaCount = result.Count().ToString();
                    var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
                    mapmanga.numberFollow = _db.Users.Count(x => x.bookmarks.Any(y => y.IdBotruyen == a.MangaId));
                    mapmanga.Listcategory = listCategory;
                    data.Add(mapmanga);
                }
                return new ResultForTopView() { numberManga = numberManga, listmanga = data };
            }
        }

        public async Task<List<botruyenViewforTopmanga>> danhSahcBotruyen(int type, int pagesize, int pagenumber, string requesurl)
        {
            List<botruyenViewforTopmanga> data = new List<botruyenViewforTopmanga>();
            IEnumerable<BoTruyen> botruyen = new List<BoTruyen>();
            switch (type)
            {
                case 0:
                    botruyen = await MangaByName(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 1:
                    botruyen = await MangaByNameDes(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 2:
                    botruyen = await MangaByStatus(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 3:
                    botruyen = await MangaByDateUpdate(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 4:
                    botruyen = await MangaByNumberOfChapter(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 5:
                    botruyen = await MangaByView(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 6:
                    botruyen = await MangaByRating(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                default:
                    botruyen = await MangaByName(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
            }
            return data;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByName(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.Where(x=> x.DeleteStatus == true).OrderBy(x => x.MangaName).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByNameDes(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.Where(x => x.DeleteStatus == true).OrderByDescending(x => x.MangaName).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByStatus(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.Where(x=> x.Status == true).OrderByDescending(x => x.Status).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByDateUpdate(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.Where(x=> x.DeleteStatus == true).OrderByDescending(x => x.ChuongTruyens.Max(x=> x.ChapterDate)).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByNumberOfChapter(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.Where(x=> x.Status== true).OrderByDescending(x => x.ChuongTruyens.Count).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByView(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.Where(x=> x.DeleteStatus == true).OrderByDescending(x => x.BotruyenViewCounts.Max(x=> x.Viewbyyear)).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByRating(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.Where(x=> x.DeleteStatus == true).OrderByDescending(x => x.RatingMangas.Max(y=> y.Rating)).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<List<botruyenViewforTopmanga>> mapMangaToMangaView(IEnumerable<BoTruyen> listdata, string requesturl)
        {
            List<botruyenViewforTopmanga> data = new List<botruyenViewforTopmanga>();
            foreach (var a in listdata)
            {
                RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                var map1 = _mapper.Map<ResponeMangaInfo>(a);
                map1.requesturl = requesturl;
                map1.routecontroller = "Truyen-tranh";
                var mapmanga = _mapper.Map<botruyenViewforTopmanga>(map1);
                mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbyyear).FirstOrDefaultAsync();
                var chaptercount = await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).CountAsync();
                var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
                mapmanga.Listcategory= listCategory;
                mapmanga.View = mangaview.ToString();
                mapmanga.chaptercount = chaptercount.ToString();
                mapmanga.mangaCount = listdata.Count().ToString();
                data.Add(mapmanga);
            }
            return data;
        }
        public async Task<int> numbermanga()
        {
            return await _db.BoTruyens.CountAsync();
        }
		//Lấy số lượng truyện tranh được đăng tải mỗi ngày
		public async Task<Dictionary<DateTime, int>> GetDailyPublishedStoryCountAsync()
		{
			return await _db.BoTruyens
							.Where(bt => bt.DeleteStatus == true && bt.Dateupdate != null)
							.GroupBy(bt => bt.Dateupdate.Value.Date)
							.Select(group => new { Date = group.Key, Count = group.Count() })
							.ToDictionaryAsync(x => x.Date, x => x.Count);
		}

		//-----------------------------------------------End-Manga----------------------------------------------------//
		//-------------------------------------------------Chapter-----------------------------------------------------//

		//Lấy danh sách tất cả chương truyện
		public async Task<IEnumerable<ChuongTruyen>> LayTatcaChuongTruyen()
        {
            var dschapter =await _db.ChuongTruyens.ToListAsync();
            return dschapter;
        }
		//Lấy tên chương
		public async Task<IEnumerable<string>> LayDanhSachTenChuong(string MangaId)
		{
			var boTruyen = MangaId;
			var x = await _db.ChuongTruyens.Where(x=>x.MangaId.Equals(MangaId)).ToListAsync();

			// Tạo và trả về danh sách chứa chỉ tên truyện
			var danhSachTenTruyen = x.Select(a => a.ChapterName).ToList();

			return danhSachTenTruyen;
		}
		//Tạo chương truyện của bộ truyện
		public async Task<bool> TaoChuongTruyen(string Chapter, List<IFormFile> MangaImage, List<string> MangaUrl)
		{
			var chuongTruyen = JsonConvert.DeserializeObject<ChuongTruyen>(Chapter);
			var random = new Random();
			var randomNumber = "";
			for (int i = 0; i < 6; i++)
			{
				randomNumber += random.Next(10).ToString();
			}
			chuongTruyen.ChapterId = "c" + randomNumber; // Gán ChapterId
			string FolderPath = _sv.OnGetFolderPath(data);
			string mangaPath = Path.Combine(FolderPath, chuongTruyen!.MangaId);
			if (_sv.CreateFolder(chuongTruyen.ChapterId, mangaPath) == true)
			{
				_db.ChuongTruyens.Add(chuongTruyen);
				await _db.SaveChangesAsync();
				return true;
			}
			return false;
		}
		//Sửa chương truyện của bộ truyện
		public async Task<bool> SuaChuongTruyen(string Chapter, List<IFormFile> MangaImage, List<string> MangaUrl)
		{
			var chuongTruyen = JsonConvert.DeserializeObject<ChuongTruyen>(Chapter);
			string FolderPath = _sv.OnGetFolderPath(data);
			string mangaPath = Path.Combine(FolderPath, chuongTruyen!.MangaId, chuongTruyen.ChapterId);
			ChuongTruyen? x = await _db.ChuongTruyens.FindAsync(chuongTruyen.ChapterId);
			if (x != null)
			{
				x.ChapterId = (chuongTruyen.ChapterId ?? x.ChapterId);
				x.ChapterName = (chuongTruyen.ChapterName ?? x.ChapterId);
				x.ChapterTitle = chuongTruyen.ChapterTitle;
				x.ChapterDate = x.ChapterDate;
				x.MangaId = chuongTruyen.MangaId;
				await _db.SaveChangesAsync();
				return true;
			}
			return false;
		}
		//Xóa chương truyện
		public async Task<bool> XoaChuongTruyen(string MangaId, string ChapterId)
		{
			var chuongtruyen = await _db.ChuongTruyens
							   .Include(ct => ct.ChapterImages)
							   .FirstOrDefaultAsync(ct => ct.ChapterId == ChapterId && ct.MangaId == MangaId);
			if (chuongtruyen != null)
			{
				using (var transaction = _db.Database.BeginTransaction())
				{
					try
					{
						string mangaFile = Path.Combine(_sv.OnGetFolderPath(data), MangaId, ChapterId);
						if (mangaFile != null) _sv.DeleteFolder(mangaFile);

						_db.ChapterImages.RemoveRange(chuongtruyen.ChapterImages);
						_db.ChuongTruyens.Remove(chuongtruyen);

						await _db.SaveChangesAsync();
						await transaction.CommitAsync();

						return true;
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						// Xử lý lỗi hoặc ghi log tại đây
						return false;
					}
				}
			}

			return false;
		}
		//Lấy danh sách chương truyện của một bộ truyện cụ thể
		public async Task<IEnumerable<chapterView2>> DanhSachChuongCuaBoTruyen(string idManga, string requestUrl, string routeController)
        {
            var dschuong =await _db.ChuongTruyens.Where(x => x.MangaId == idManga).ToListAsync();
            var a = new List<chapterView2>();
            foreach (var item in dschuong)
            { 
                var chap= _mapper.Map<chapterView2>(item);
                a.Add(chap);
            }
            return a;
        }
		//Lấy chương truyện của một bộ truyện cụ thể
		public async Task<chapterView2> ThongTinChuongTruyen(string idManga, string chapterId, string requestUrl, string routeController)
		{
			var chuongTruyen = await _db.ChuongTruyens
									   .Where(x => x.MangaId == idManga && x.ChapterId == chapterId)
									   .FirstOrDefaultAsync();

			if (chuongTruyen == null)
			{
				throw new KeyNotFoundException("Chương truyện không tồn tại.");
			}

			var chapterView = _mapper.Map<chapterView2>(chuongTruyen);
			return chapterView;
		}

		//Lấy danh sách ảnh của chương truyện
		public async Task<IEnumerable<string>> DanhSachAnhTheoChuong(string idManga, string idChapter, string requestUrl, string routeController)
        {
            string[] imageName = _db.ChapterImages.Where(x => x.ChapterId == idChapter).Select(x => x.ImageName).ToArray();
            for (int i = 0; i < imageName.Length; i++)
            {
                string a = _sv.getUrlImageforChapter(requestUrl, routeController, idManga, idChapter, imageName[i]).Replace("http:", "https:");
                imageName[i] = (await _sv.checkUrlImage(a)) ? a : imageName[i];
            }
            return imageName;
        }
		//Cập nhập ảnh chương
		public async Task<bool> UploadChapterImages(string Chapter, List<IFormFile> MangaImage, List<string> MangaUrl)
		{
			var chuongTruyen = JsonConvert.DeserializeObject<ChuongTruyen>(Chapter);
			string FolderPath = _sv.OnGetFolderPath(data);
			string mangaPath = Path.Combine(FolderPath, chuongTruyen.MangaId, chuongTruyen.ChapterId);


			// Lấy index lớn nhất hiện tại từ cơ sở dữ liệu
			var maxIndex = _db.ChapterImages
				  .Where(ci => ci.ChapterId == chuongTruyen.ChapterId)
				  .Select(ci => ci.ImageIndex)
				  .ToList() // Hoặc .ToArray()
				  .DefaultIfEmpty(0)
				  .Max();


			foreach (var image in MangaImage)
			{
				await _sv.UpLoadimage(image, mangaPath);
				var chapterImage = new ChapterImage
				{
					ImageName = image.FileName,
					ImageUl = null,
					ChapterId = chuongTruyen.ChapterId,
					ImageIndex = ++maxIndex
				};
				_db.ChapterImages.Add(chapterImage);
			}

			foreach (var url in MangaUrl)
			{
				if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("http"))
				{
					var chapterImage = new ChapterImage
					{
						ImageName = url, // Giả định rằng 'url' chứa tên hình ảnh
						ImageUl = url,
						ChapterId = chuongTruyen.ChapterId,
						ImageIndex = ++maxIndex
					};
					_db.ChapterImages.Add(chapterImage);
				}
			}

			await _db.SaveChangesAsync();
			return true;
		}
		//Xóa ảnh chương
		public async Task<bool> DeleteChapterImage(string mangaId, string chapterId, int imageId)
		{
			var chapterImage = await _db.ChapterImages
										.Include(ci => ci.Chapter)
										.Where(ci => ci.ImageId == imageId && ci.Chapter.ChapterId == chapterId && ci.Chapter.MangaId == mangaId)
										.FirstOrDefaultAsync();

			if (chapterImage == null)
			{
				return false; // Không tìm thấy ảnh
			}

			// Xóa file ảnh từ hệ thống file nếu cần
			string FolderPath = _sv.OnGetFolderPath(data);
			var filePath = Path.Combine(FolderPath, mangaId, chapterImage.ImageName);
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			// Xóa ảnh từ cơ sở dữ liệu
			_db.ChapterImages.Remove(chapterImage);
			await _db.SaveChangesAsync();

			return true; // Xóa thành công
		}

		//Lấy tất ca thông tin ảnh chương
		public async Task<IEnumerable<ChapterImageModel>> GetAllImageInChapter (string idManga, string idChapter, string requestUrl, string routeController)
		{
			var images = _db.ChapterImages
							.Where(x => x.ChapterId == idChapter)
							.OrderBy(x => x.ImageIndex) // Sắp xếp dựa trên ImageIndex
							.Select(x => new ChapterImageModel
							{
								ImageId = x.ImageId,
								ImageName = x.ImageName,
								ImageUrl = _sv.getUrlImageforChapter(requestUrl, routeController, idManga, idChapter, x.ImageName).Replace("http:", "https:"),
								ImageIndex = x.ImageIndex // Lấy thông tin vị trí
							}).ToList();

			foreach (var image in images)
			{
				image.ImageUrl = (await _sv.checkUrlImage(image.ImageUrl)) ? image.ImageUrl : image.ImageName;
			}

			return images;
		}

		//Sửa vị trí ảnh
		public async Task UpdateImagePositions(string mangaId, string chapterId, List<ImagePositionUpdateModel> imageUpdates)
		{
			foreach (var update in imageUpdates)
			{
				var image = await _db.ChapterImages
					.Where(img => img.ImageId == update.ImageId && img.ChapterId == chapterId && img.Chapter.MangaId == mangaId)
					.FirstOrDefaultAsync();

				if (image != null)
				{
					image.ImageIndex = update.NewPosition;
				}
			}

			await _db.SaveChangesAsync();
		}


		//Lấy ảnh của chương truyện
		public string LayAnh(string idManga, string idChapter, string image)
        {
            string imagePath = Path.Combine(_env.ContentRootPath, "Manga", idManga, idChapter, image);
            return imagePath;
        }

        //------------------------------------------------End-Chapter----------------------------------------------------//
        //-------------------------------------------------Categories-----------------------------------------------------//
        //Lấy danh sách thể loại
        public async Task<List<ResponeCategory>> getListCategory()
        {
            using(var _context= _db)
            {
                var data =await _context.TheLoais.ToListAsync();
                var map = _mapper.Map<List<ResponeCategory>>(data);  
                return map;
            }
        }
		//Thêm thể loại
		public async Task<bool> AddTheLoai(CategoryAddedit categoryAddedit)
		{
			try
			{

				// Tìm giá trị GenreId lớn nhất hiện tại và tăng nó lên 1
				//int newGenreId = _db.TheLoais.Any() ? _db.TheLoais.Max(tl => tl.GenreId) + 1 : 1;

				var theLoai = new TheLoai
				{
					// Giả sử GenreId được tạo tự động hoặc không cần thiết
					//GenreId = newGenreId,
					GenresIdName = categoryAddedit.GenresIdName,
					Info = categoryAddedit.Info
				};

				_db.TheLoais.Add(theLoai);
				await _db.SaveChangesAsync();
				return true;
			}
			catch
			{
				// Trả về false nếu có lỗi xảy ra khi thêm dữ liệu vào cơ sở dữ liệu
				return false;
			}
		}

		//Xóa thể loại
		public async Task<bool> DeleteTheLoai(int genreId)
		{
			var theLoai = await _db.TheLoais
										.Include(tl => tl.Mangas)
										.FirstOrDefaultAsync(tl => tl.GenreId == genreId);

			if (theLoai == null)
			{
				// Không tìm thấy TheLoai
				return false;
			}

			if (theLoai.Mangas.Any())
			{
				// Không thể xóa TheLoai vì có BoTruyen liên kết
				return false;
			}

			_db.TheLoais.Remove(theLoai);
			await _db.SaveChangesAsync();
			return true;
		}
		//Sửa thể loại
		public async Task<bool> UpdateTheLoai(int genreId, CategoryAddedit categoryAddedit)
		{
			try
			{
				var theLoai = await _db.TheLoais.FindAsync(genreId);
				if (theLoai == null)
				{
					// Không tìm thấy TheLoai
					return false;
				}

				// Cập nhật thông tin
				theLoai.GenresIdName = categoryAddedit.GenresIdName;
				theLoai.Info = categoryAddedit.Info;

				_db.TheLoais.Update(theLoai);
				await _db.SaveChangesAsync();
				return true;
			}
			catch
			{
				// Trả về false nếu có lỗi xảy ra khi cập nhật dữ liệu vào cơ sở dữ liệu
				return false;
			}
		}
		//Lấy danh sách truyện theo thể loại truyện
		public async Task<ResultForMangaView> getMangaByCategory(string id,string pagenumber, string pagesize, string requestUrl)
        {
            using (var _context = _db)
            {
                int idx = int.Parse(id);
                int pageNumber = int.Parse(pagenumber);
                int pageSize = int.Parse(pagesize);
                List<ResponeManga> data = new List<ResponeManga>();
                var mangasInGenre = await _context.TheLoais
                .Where(tl => tl.GenreId == idx) // Thay yourGenreId bằng ID của thể loại bạn quan tâm
                .SelectMany(tl => tl.Mangas.Where(x=> x.DeleteStatus == true))
                .ToPagedListAsync(pageNumber, pageSize);
                var mangacount = _context.TheLoais
                .Where(tl => tl.GenreId == idx)
                .SelectMany(tl => tl.Mangas).Count();
                foreach (var item in mangasInGenre)
                {
                    RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", item.MangaId).FirstOrDefaultAsync();
                    var map1 = _mapper.Map<ResponeMangaInfo>(item);
                    map1.requesturl = requestUrl;
                    map1.routecontroller = "Truyen-tranh";
                    var mapmanga = _mapper.Map<ResponeManga>(map1);
                    mapmanga.Rating = rating?.Rating.ToString() ?? "0";
                    List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == item.MangaId).OrderByDescending(y => y.ChapterDate)
                        .Take(3).ToListAsync();
                    var listCategory = await _db.BoTruyens.Where(x => x.MangaId == item.MangaId).SelectMany(y => y.Genres).ToListAsync();
                    var mapchapter = _mapper.Map<List<chapterView2>>(listChapter);
                    mapmanga.ListChaper = mapchapter;
                    mapmanga.Listcategory = listCategory;
                    data.Add(mapmanga);
                }

                return new ResultForMangaView() {numberManga= mangacount, listmanga= data };
            }
        }
        //Tìm kiếm truyện nâng cao theo thể loại
        public async Task<List<ResponeManga>> getMangaByCategories(List<string> listCategories, string requestUrl, string routeController)
        {
            //var result = await _db.BoTruyens
            //            .Where(botruyen => listCategories.All(x => botruyen.Genres.Any(y => y.GenreId.ToString() == x)))
            //            .ToListAsync();

            var result = await (from botruyen in _db.BoTruyens
                                where botruyen.Genres.Any(x => listCategories.Contains(x.GenreId.ToString())) 
                                && botruyen.DeleteStatus == true
                                select botruyen).ToListAsync();
            List<ResponeManga> a = new List<ResponeManga>();
            foreach(var item in result)
           {
                RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", item.MangaId).FirstOrDefaultAsync();
                var map1 = _mapper.Map<ResponeMangaInfo>(item);
                map1.requesturl = requestUrl;
                map1.routecontroller = routeController;
                var mapmanga = _mapper.Map<ResponeManga>(map1);
                mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == item.MangaId).OrderByDescending(y => y.ChapterDate)
                    .Take(3).ToListAsync();
                var listCategory = await _db.BoTruyens.Where(x => x.MangaId == item.MangaId).SelectMany(y => y.Genres).ToListAsync();
                var mapchapter = _mapper.Map<List<chapterView2>>(listChapter);
                mapmanga.ListChaper = mapchapter;
                mapmanga.Listcategory = listCategory;
                a.Add(mapmanga);
            }
            return a;

        }
        //Tìm kiếm truyện nâng cao theo tất cả thể loại
        public async Task<string> getMangaByCategoriesAll(List<string> listCategories, string requestUrl)
        {
            //var test = await (from theloai in _db.TheLoais
            //                  where listCategories.All(x => theloai.GenreId.Equals(x))
            //                  select theloai.Mangas
            //                  ).ToListAsync();
            List<BoTruyen> listmanga = await _db.BoTruyens
                .Include(boTruyen => boTruyen.Genres)
                .Include(chapter => chapter.ChuongTruyens.Take(3))
                .Where(x=> x.DeleteStatus == true)
                .ToListAsync();
            var result = listmanga.Where(x => listCategories.All(y => x.Genres.Any(z => z.GenreId == int.Parse(y)))).ToList();
            List<ResponeManga> a = new List<ResponeManga>();
            foreach (var item in result)
            {
                RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", item.MangaId).FirstOrDefaultAsync();
                var map1 = _mapper.Map<ResponeMangaInfo>(item);
                map1.requesturl = requestUrl;
                map1.routecontroller = "Truyen-tranh";
                var mapmanga = _mapper.Map<ResponeManga>(map1);
                mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                //List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == item.MangaId).OrderByDescending(y => y.ChapterDate)
                //    .Take(3).ToListAsync();
                //var listCategory = await _db.BoTruyens.Where(x => x.MangaId == item.MangaId).SelectMany(y => y.Genres).ToListAsync();
                var mapchapter = _mapper.Map<List<chapterView2>>(item.ChuongTruyens.ToList());
                mapmanga.ListChaper = mapchapter;
                mapmanga.Listcategory = item.Genres.ToList();
                a.Add(mapmanga);
            }
            var data = System.Text.Json.JsonSerializer.Serialize(a, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            });
            return data;

        }
        //Tải danh sách topmanga default
        public async Task<List<TopManga>> getTopMangaDefault(string requestUrl, string routeController)
        {
            var date = Topmangabydate(null, requestUrl, routeController);
            var month = Topmangabymonth(null, requestUrl, routeController);
            var year = TopmangabyYear(null, requestUrl, routeController);
            await Task.WhenAll(date, month, year);
            List<TopManga> data = new List<TopManga>();

            data.AddRange(date.Result);
            data.AddRange(month.Result);
            data.AddRange(year.Result);
            return data;
        }
        //lấy danh sách top manga theo ngày
        public async Task<List<TopManga>> Topmangabydate(int? page, string requestUrl, string routeController)
        {
            int pageNumber = (page ?? 1);
            int pagesize = 10;
            var result =await (from botruyen in _db.BoTruyens
                          join view in _db.ViewCounts
                          on botruyen.MangaId equals view.Id
                          //where botruyen.Status == true
                          select new BoTruyenTopView
                          {
                              MangaId = botruyen.MangaId,
                              MangaName = botruyen.MangaName,
                              MangaImage = botruyen.MangaImage,
                              View = view.Viewbydate,
                              requesturl= requestUrl,
                              routecontroller = routeController,
                          }).OrderByDescending(x => x.View).ToPagedListAsync(pageNumber, pagesize);
            var map= _mapper.Map<List<TopManga>>(result);
            map.ForEach(item => item.Typetop = "1");
            return map;
        }
        //lấy danh sách top manga theo tháng
        public async Task<List<TopManga>> Topmangabymonth(int? page, string requestUrl, string routeController)
        {
            int pageNumber = (page ?? 1);
            int pagesize = 10;
            var result = await (from botruyen in _db.BoTruyens
                                join view in _db.ViewCounts
                                on botruyen.MangaId equals view.Id
                                where botruyen.DeleteStatus == true
                                select new BoTruyenTopView
                                {
                                    MangaId = botruyen.MangaId,
                                    MangaName = botruyen.MangaName,
                                    MangaImage = botruyen.MangaImage,
                                    View = view.Viewbymonth,
                                    requesturl = requestUrl,
                                    routecontroller = routeController,
                                }).OrderByDescending(x => x.View).ToPagedListAsync(pageNumber, pagesize);
            var map = _mapper.Map<List<TopManga>>(result);
            map.ForEach(item => item.Typetop = "2");
            return map;
        }
        //lấy danh sách top manga theo năm
        public async Task<List<TopManga>> TopmangabyYear(int? page, string requestUrl, string routeController)
        {
            int pageNumber = (page ?? 1);
            int pagesize = 10;
            var result = await (from botruyen in _db.BoTruyens
                                join view in _db.ViewCounts
                                on botruyen.MangaId equals view.Id
                                where botruyen.DeleteStatus == true
                                select new BoTruyenTopView
                                {
                                    MangaId = botruyen.MangaId,
                                    MangaName = botruyen.MangaName,
                                    MangaImage = botruyen.MangaImage,
                                    View = view.Viewbyyear,
                                    requesturl = requestUrl,
                                    routecontroller = routeController,
                                }).OrderByDescending(x => x.View).ToPagedListAsync(pageNumber, pagesize);
            var map = _mapper.Map<List<TopManga>>(result);
            map.ForEach(item => item.Typetop = "3");
            return map;
        }


		//------------------------------------------------End-Categories----------------------------------------------------//
		//-------------------------------------------------TypeMangas-----------------------------------------------------//

		//Lấy tất cả kiểu truyện
		public IEnumerable<TypeManga> GetAllTypeMangas()
		{
			return _db.TypeMangas.ToList();
		}
		//Thêm kiểu truyện
		public async Task<bool> AddTypeManga(ResponeType typeMangaDTO)
		{
			var typeManga = new TypeManga
			{
				Name = typeMangaDTO.Name
			};

			_db.TypeMangas.Add(typeManga);
			int result = await _db.SaveChangesAsync();
			return result > 0;
		}
		//Xóa kiểu truyện
		public async Task<bool> DeleteTypeManga(int id)
		{
			var typeManga = await _db.TypeMangas.FindAsync(id);
			if (typeManga == null)
			{
				return false;
			}

			_db.TypeMangas.Remove(typeManga);
			int result = await _db.SaveChangesAsync();
			return result > 0;
		}
		//Sửa kiểu truyện
		public async Task<bool> UpdateTypeManga(ResponeType typeMangaDTO)
		{
			var existingTypeManga = await _db.TypeMangas.FindAsync(typeMangaDTO.Id);
			if (existingTypeManga == null)
			{
				return false;
			}

			existingTypeManga.Name = typeMangaDTO.Name;
			_db.TypeMangas.Update(existingTypeManga);
			int result = await _db.SaveChangesAsync();
			return result > 0;
		}

	}
}
