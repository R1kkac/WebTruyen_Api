using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Repositories;
using TestWebApi_v1.Service.Hubs;
using X.PagedList;

namespace TestWebApi_v1.Service
{
    public class BackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly WebTruyenTranh_v2Context _db = new WebTruyenTranh_v2Context();
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _cache;
        private readonly ChatRealTimeService _chat;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IServiceRepo _sv;
		private static string data = "Manga";
		public BackgroundService(IMemoryCache memoryCache, IDistributedCache cache, ChatRealTimeService chat, IServiceRepo sv, IServiceScopeFactory serviceScopeFactory)

		{
            _memoryCache = memoryCache;
            _cache = cache;
            _chat = chat;
			_sv = sv;
			_serviceScopeFactory = serviceScopeFactory;
		}
        /// <summary>
        /// Cẩn thận khai báo các dependency injection vì phạm vi của mybackgroundservice là Singleton còn có thể các dịch vụ khác
        /// là scoped hay Transient
        /// {Singleton: Dịch vụ chỉ được tạo một lần và được sử dụng trong suốt vòng đời của ứng dụng.
        /// Scoped: Dịch vụ được tạo mỗi khi một phạm vi định rõ bắt đầu(thường là mỗi yêu cầu HTTP).
        /// Transient: Dịch vụ được tạo mỗi khi được yêu cầu.}
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

			// Hàm sẽ chạy ngay khi ứng dụng khởi động
			var lastRunTime = DateTime.UtcNow;
			while (!stoppingToken.IsCancellationRequested)
            {
                // Thực hiện công việc cần chạy
                int pageSize = 10; // Số lượng bản ghi trên mỗi trang
                int pageNumber = 1; // Trang hiện tại
                var dsbotruyen = await _db.BoTruyens.Where(x=> x.DeleteStatus == true).AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
                //var result = new List<BoTruyen>();
                //foreach (var a in dsbotruyen)
                //{
                //    BoTruyen b = new BoTruyen
                //    {
                //        MangaId = a.MangaId,
                //        MangaName = a.MangaName,
                //        MangaDetails = a.MangaDetails,
                //        MangaImage = (a.MangaImage != null) ? $"https://localhost:7132/Truyen-tranh/{a.MangaId}/{a.MangaImage}" : null,
                //        MangaAlternateName = a.MangaAlternateName,
                //        MangaAuthor = a.MangaAuthor,
                //        MangaArtist = a.MangaArtist,
                //        Type = a.Type,
                //    };
                //    result.Add(b);
                //}
                //cái này lưu vào bộ nhớ cục bộ
                _memoryCache.Set("ListManga", dsbotruyen);
				//bộ nhớ phân táng
				//_cache.SetString("ListManga", result.ToString());

				using (var scope = _serviceScopeFactory.CreateScope())
				{
					var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
					if (DateTime.UtcNow - lastRunTime >= TimeSpan.FromDays(1))
					{
						await DeleteMarkedStories(userManager);
						lastRunTime = DateTime.UtcNow; // Cập nhật thời gian chạy cuối
					}
				}

				await MangaviewCount.PushViewToDatabase();
                await ChatManager.SaveChatToDatabase();

                // Đợi một khoản thời gian trước khi lặp lại
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); 
            }
        }
		// Logic xóa các truyện đã vào thùng rác (3 ngày)
		private async Task DeleteMarkedStories(UserManager<User> userManager)
		{
			var deleteThreshold = DateTime.UtcNow.AddDays(-3);//Để test dùng AddMinutes hoặc AddSeconds
			var mangasToDelete = _db.BoTruyens
				.Include(bt => bt.Genres)
				.Include(bt => bt.ChatRooms)
				.Include(bt => bt.BinhLuans)
					.ThenInclude(bl => bl.ReplyComments)
				.Include(bt => bt.RatingMangas)
				.Include(bt => bt.BotruyenViewCounts)
				.Include(bt => bt.IdNavigation)
				.Where(s => s.Status == false && s.MarkedAsDeletedDate <= deleteThreshold)
				.ToList();

			foreach (var truyen in mangasToDelete)
			{
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

				// Xóa liên kết từ các TheLoai
				foreach (var theLoai in truyen.Genres.ToList())
				{
					theLoai.Mangas.Remove(truyen);
				}

				// Xóa các chương truyện và ảnh liên quan
				string mangaFile = Path.Combine(_sv.OnGetFolderPath(data), truyen.MangaId);
				if (mangaFile != null) _sv.DeleteFolder(mangaFile);

				var chuongTruyens = _db.ChuongTruyens.Where(x => x.MangaId == truyen.MangaId).ToList();
				foreach (var chuong in chuongTruyens)
				{
					var images = _db.ChapterImages.Where(x => x.ChapterId == chuong.ChapterId);
					_db.ChapterImages.RemoveRange(images);
				}
				_db.ChuongTruyens.RemoveRange(chuongTruyens);

				//Xóa các mối liên kết user với bộ truyện
				var userId = truyen.Id;
				var user = await userManager.FindByIdAsync(userId);
				if (user != null)
				{
					user.BoTruyens.Remove(truyen);
				}
				// Xóa BoTruyen
				_db.BoTruyens.Remove(truyen);

				string mangaImagePath = Path.Combine(_sv.OnGetFolderPath(data), "Content");
				_sv.DeleteImage(mangaImagePath + "\\" + truyen.MangaImage);
			}

			if (mangasToDelete.Any())
			{
				await _db.SaveChangesAsync();
			}
		}
	}
    //Auto save data for Chat-real-time
    public class MyBackgroundService2 : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly ChatRealTimeService _chat;
        public MyBackgroundService2(ChatRealTimeService chat)
        {
            _chat = chat;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _chat.CloseAndSaveChat();
                await ChatManager.AuToCloseChat();
                //var curtime = DateTime.Now.Hour;
                //var timespan = 24 - curtime;
                //await Task.Delay(TimeSpan.FromHours(timespan), stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            }
        }
    }
}
