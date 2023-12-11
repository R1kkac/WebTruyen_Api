using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using X.PagedList;

namespace TestWebApi_v1.Repositories
{
    public class MyBackgroundService: BackgroundService
    {
        private readonly WebTruyenTranh_v2Context _db = new WebTruyenTranh_v2Context();
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _cache;
        public MyBackgroundService(IMemoryCache memoryCache ,IDistributedCache cache)
        { 
            _memoryCache = memoryCache;
            _cache = cache;
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
            while (!stoppingToken.IsCancellationRequested)
            {
                // Thực hiện công việc cần chạy
                int pageSize = 10; // Số lượng bản ghi trên mỗi trang
                int pageNumber = 1; // Trang hiện tại
                var dsbotruyen = await _db.BoTruyens.AsNoTracking().ToPagedListAsync(pageNumber, pageSize);
                var result = new List<botruyenView>();
                foreach (var a in dsbotruyen)
                {
                    botruyenView b = new botruyenView
                    {
                        MangaId = a.MangaId,
                        MangaName = a.MangaName,
                        MangaDetails = a.MangaDetails,
                        MangaImage = (a.MangaImage != null) ? $"https://localhost:7132/Truyen-tranh/{a.MangaId}/{a.MangaImage}" : null,
                        MangaAlternateName = a.MangaAlternateName,
                        MangaAuthor = a.MangaAuthor,
                        MangaArtist = a.MangaArtist,
                        MangaGenre = a.MangaGenre
                    };
                    result.Add(b);
                }
                //cái này lưu vào bộ nhớ cục bộ
                //_memoryCache.Set("ListManga", result);
                //bộ nhớ phân táng
                //_cache.SetString("ListManga", result.ToString());
                
                await MangaviewCount.PushViewToDatabase();


                // Đợi một khoản thời gian trước khi lặp lại
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // 10 giây
            }
        }
    }
}
