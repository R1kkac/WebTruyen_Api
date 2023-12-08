using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Security.Claims;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel.MangaView;
using TestWebApi_v1.Repositories;
using TestWebApi_v1.Service;
using TestWebApi_v1.Service.Hubs;

namespace TestWebApi_v1.Controllers
{
    [Route("Truyen-tranh")]
    [ApiController]
    public class MangaController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private ILogger<MangaController> _logger;
        private readonly IMangaModel _mangaModel;
        private readonly IHubContext<ThongBaoNguoiDung> _tb;
        private readonly ThongBaoNguoiDung _tb2;
        private readonly UserManager<User> _userManager;
        public MangaController(IMangaModel mangaModel, IHubContext<ThongBaoNguoiDung> tb, ILogger<MangaController> logger,
            IMemoryCache memorycache,UserManager<User> userManager,ThongBaoNguoiDung tb2)
        {
            _mangaModel = mangaModel;
            _tb = tb;
            _logger = logger;
            _memoryCache = memorycache;
            _userManager = userManager;
            _tb2 = tb2;

        }
        //[HttpPost("ChatHub")]
        //public async Task<IActionResult> chatRealTIme([FromForm] string message, [FromForm]string IdUser)
        //{
        //    //User user=await _userManager.FindByIdAsync(IdUser);
        //    // await _tb.Clients.User(IdUser).SendAsync("message", message);
        //    //await _tb2.chatRealTime(message, IdUser);
        //    return Ok();
        //}
        //Bộ truyện//
        [HttpGet("SearchManga")]
        public async Task<IEnumerable<Searchmanga>> searManga()
        {
            return await _mangaModel.search();
        }
        [HttpGet("MangaNewUpdate")]
        public async Task<List<botruyenView>> getMangaUpdate() 
        {
            var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
            string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result =await _mangaModel.getMangaNewUpdate(requestUrl, routeController);
            return result;
        }
        [HttpGet("SearchMangaV2/{value}")]
        public async Task<IEnumerable<Searchmanga>> searchmangaV2(string value)
        {
            var url = getCurrenthttpContext();
            return await _mangaModel.responeSearch(value, url);
        }
        [HttpGet("Gettopmanga/{type}")]
        public async Task<List<botruyenViewforTopmanga>> getTOpManga(string type)
        {
            var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
            string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result =await _mangaModel.getTopmanga(type, requestUrl, routeController);
            return result;
        }
        [HttpGet("GetAllManga/{page}")]
        public async Task<IActionResult> GetManga(int? page)
        { 
            try
            {
                Stopwatch t = new Stopwatch();
                t.Start();
                var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
                string routeController = (routeAttribute!=null)? routeAttribute.Template:"";
                string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
                var result=await _mangaModel.LayDanhSachTruyenTheoPage(page, requestUrl, routeController);


                _memoryCache.Set("cachedData",result);
                t.Stop();
                Console.WriteLine("Eslaped Time QueryDatabase: " + t.Elapsed);
                return Json(result);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        //Lấy thông tin của một bộ truyện cụ thể
        [HttpGet("Details/{MangaId}")]
        public async Task<IActionResult> getSimpleManga(string MangaId)
        {
            try
            {
                var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
                string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
                string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
                var result= await _mangaModel.LayThongTinTruyen(MangaId, requestUrl, routeController);
                return Json(result);
            }
            catch(Exception)
            {
                return NotFound();
            }
        }

        //Lấy ảnh bìa của bộ truyện
        [HttpGet("{idManga}/{imageManga}")]
        public IActionResult MangaImage(string imageManga)
        {
            var result = _mangaModel.LayAnhTruyen(imageManga);
            return PhysicalFile(result!, "image/jpeg"); 
        }

        ////Tạo bộ truyện
        [Authorize(Roles = "Admin,Upload")]
        [HttpPost("Create")]
        //[EnableCors("Policy")]
        //public async Task<IActionResult> CreateManga([FromForm] string MangaId, [FromForm] string MangaName, [FromForm] string MangaDetails,
        //   [FromForm] IFormFile? MangaImage, [FromForm] string MangaAlternateName, [FromForm] string MangaAuthor, [FromForm] string MangaArtist, [FromForm] string MangaGenre)
        public async Task<IActionResult> CreateManga([FromForm] string Manga, [FromForm] IFormFile? MangaImage)
        {
            try
            {
                var Id = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
                var result = await _mangaModel.TaoTruyen(Id, Manga, MangaImage);
                if(result) {
                    return StatusCode(StatusCodes.Status201Created,
                     new Respone { Status = "Success", Message = $"đã tạo thành công"});
                }
                return StatusCode(StatusCodes.Status417ExpectationFailed,
                     new Respone { Status = "Failed", Message = $"Không thể tạo truyện" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status424FailedDependency,
                     new Respone { Status = "Error", Message = $"Đã gặp lỗi trong quá trình thêm truyện" });
            }
        }

        //Sửa thông tin của bộ truyện
        [Authorize(Roles = "Admin,Upload")]
        [HttpPut("EditManga/{MangaId}")]
        //[EnableCors("Policy")]
        public async Task<IActionResult> EditManga([FromForm] string Manga,[FromForm] IFormFile? MangaImage)
        {
            try
            {
                var Id = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
                var result= await _mangaModel.SuaTruyen(Id, Manga, MangaImage);
                if (result)
                {
                    return StatusCode(StatusCodes.Status201Created,
                     new Respone { Status = "Success", Message = $"Truyện đã được sửa thành công" });
                }
                return StatusCode(StatusCodes.Status417ExpectationFailed,
                  new Respone { Status = "Failed", Message = $"Truyện sửa thất bại" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Respone { Status = "Error", Message = $"Đã gặp lỗi trong quá trình sửa" });
            }
        }

        //Xóa bộ truyện
        [Authorize(Roles = "Admin,Upload")]
        [EnableCors("Policy")]
        [HttpDelete("Delete/{MangaId}")]
        public  async Task<IActionResult> DeleteManga(string MangaId)
        {
            try
            {
                var Id = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
                var result= await _mangaModel.XoaTruyen(Id, MangaId);
                if (result)
                {
                    return StatusCode(StatusCodes.Status200OK,
                            new Respone { Status = "Success", Message = $"Xóa thành công" });
                }
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Failed", Message = $"Xóa thất bại" });
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Error", Message = $"Gặp lỗi trong quá trình xóa" });
            }
        }

        //End Bộ truyện//
        //Chương truyện//

        //Lấy danh sách tất cả chương truyện
        [HttpGet("GetdsChapter")]
        public async Task<IActionResult> layDanhsachChapter()
        {
            try
            {
                var result = await _mangaModel.LayTatcaChuongTruyen();
                return Json(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Error", Message = $"Gặp lỗi trong quá trình lấy dữ liệu" });
            }
        }

        //Tạo chương truyện của bộ truyện
        [Authorize(Roles = "Admin,Upload")]
        [EnableCors("Policy")]
        [HttpPost("{MangaId}/CreateChapter")]
        //public async Task<IActionResult> CreateChapter([FromForm] string MangaId, [FromForm] string ChapterId,
        //    [FromForm] string tenChuong, [FromForm] string TieuDe, [FromForm] List<IFormFile> MangaImage, [FromForm] List<string> MangaUrl)
        public async Task<IActionResult> CreateChapter([FromForm] string Chapter, [FromForm] List<IFormFile> MangaImage, [FromForm] List<string> MangaUrl)
        {
            try
            {
                var result= await _mangaModel.TaoChuongTruyen(Chapter, MangaImage, MangaUrl);
                if (result)
                {
                    return StatusCode(StatusCodes.Status200OK,
                            new Respone { Status = "Success", Message = $"Thành công" });
                }
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Failed", Message = $"Thất bại" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Error", Message = $"Đã xảy ra lỗi" });
            }
        }

        //Sửa chương truyện của bộ truyện
        [Authorize(Roles = "Admin,Upload")]
        [EnableCors("Policy")]
        [HttpPut("{MangaId}/{ChapterId}/EditChapter")]
        public async Task<IActionResult> editChapter([FromForm] string Chapter, [FromForm] List<IFormFile> MangaImage,[FromForm] List<string> MangaUrl)
        {
            try
            {
                var result= await _mangaModel.SuaChuongTruyen(Chapter, MangaImage,MangaUrl);
                if (result)
                {
                    return StatusCode(StatusCodes.Status200OK,
                            new Respone { Status = "Success", Message = $"Thành công" });
                }
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Failed", Message = $"Thất bại" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Error", Message = $"Đã xảy ra lỗi" });
            }
        }

        //Xóa chương truyện
        [Authorize(Roles = "Admin,Upload")]
        [EnableCors("Policy")]
        [HttpDelete("{MangaId}/{ChapterId}/DeleteChapter")]
        public async Task<IActionResult> deleteChapter(string MangaId ,string ChapterId)
        {
            try
            {
                var result= await _mangaModel.XoaChuongTruyen(MangaId,ChapterId);
                if (result)
                {
                    return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Success", Message = $"Xóa thành công" });
                }
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Failes", Message = $"Xóa thất bại" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Error", Message = $"Đã xảy ra lỗi" });
            }
        }

        //Lấy danh sách chương truyện của một bộ truyện cụ thể
        [HttpGet("{idManga}/GetChapter")]
        public async Task<IActionResult> getListChapterforMangaAsync(string idManga)
        {
            try
            {
                var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
                string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
                string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
                var result = await _mangaModel.DanhSachChuongCuaBoTruyen(idManga, requestUrl, routeController);
                if (result.Any()==false ||result != null)
                {
                    return Json(result);
                }
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Failed", Message = $"Truy xuất thất bại" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                            new Respone { Status = "Error", Message = $"Gặp lỗi trong quá trình lấy dữ liệu" });
            }
        }

        //Lấy danh sách ảnh của chương truyện
        [HttpGet("{idManga}/{idChapter}/getDsImage")]
        public async Task<IActionResult> getdsAnhChapter(string idManga, string idChapter)
        {
            try
            {
                var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
                string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
                string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
                var result = await _mangaModel.DanhSachAnhTheoChuong(idManga, idChapter, requestUrl, routeController);
                if(result.Any()==false ||result != null) { 
                    return Json(result); 
                }
                return StatusCode(StatusCodes.Status404NotFound,
                             new Respone { Status = "Failed", Message = $"Truy xuất thất bại" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                             new Respone { Status = "Error", Message = $"Gặp lỗi trong quá trình truy xuất" });
            }
        }

        //Lấy ảnh của chương truyện
        [EnableCors("Policy")]
        [HttpGet("{idManga}/{idChapter}/{image}")]
        public IActionResult getListImageChapter(string idManga, string idChapter,string image)
        {
            var result = _mangaModel.LayAnh(idManga, idChapter, image);
            if (System.IO.File.Exists(result))
            {
                return PhysicalFile(result, "image/jpeg");
            }
            return StatusCode(StatusCodes.Status404NotFound,
                              new Respone { Status = "Failed", Message = $"Not Found" });
        }


        //lấy danh sách manga theo thể loại
        [HttpGet("GetmangabyCategory/{idCategory}")]
        public async Task<List<botruyenView>> getMangabyCategry(string idCategory)
        {
            var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
            string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result = await _mangaModel.getMangaByCategory(idCategory, requestUrl, routeController);
            return result;
        }
        //lấy danh sách thể loại
        [HttpGet("Category/Getall")]
        public async Task<List<CategoryView>> getCategories()
        {
            var result =await _mangaModel.getListCategory();
            return result;
        }
        //lấy danh sách trang
        [HttpGet("GetPageNumber")]
        public async Task<int> getPageNumber()
        {
            return await _mangaModel.getPageNumber();
        }
        //Lấy manga theo danh sách thể loại
        [HttpGet("GetMangaByCategories")]
        public async Task<List<botruyenView>> GetMangaByCategories([FromQuery] List<string> List)
        {
            var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
            string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result =await _mangaModel.getMangaByCategories(List, requestUrl, routeController);
            return result; ;
        }
        //Lấy topmanga default
        [HttpGet("Topmanga")]
        public async Task<List<TopManga>> defaultTopManga()
        {
            var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
            string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result =await _mangaModel.getTopMangaDefault(requestUrl, routeController);
            return result;
        }
        private string getCurrenthttpContext()
        {
            var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
            string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result= $"{requestUrl}{routeController}";
            return result;
        }
        [HttpGet("LayThuCache")]
        public async Task<IEnumerable<botruyenView>> getdata( )
        {
            if (_memoryCache.TryGetValue("cachedData", out List<botruyenView> cachedData))
            {
                Stopwatch t = new Stopwatch(); ;
                t.Start();
                // Dữ liệu được tìm thấy trong cache, sử dụng cachedData ở đây
                //_logger.LogInformation($"Data found in cache: {cachedData.Count} items");
                //Console.WriteLine($"Data found in cache: {cachedData.Count} items");

                //// Thực hiện các thao tác với dữ liệu từ cache
                //foreach (var item in cachedData)
                //{
                //    Console.WriteLine(item);
                //}
                t.Stop();
                Console.WriteLine("Eslaped Time Cache: " + t.Elapsed);
                return cachedData;
            }
            else
            {
                Console.WriteLine("Eo tim thay gi");
                return null;
                // Dữ liệu không được tìm thấy trong cache
                // Thực hiện các thao tác để lấy dữ liệu từ nguồn dữ liệu chính và sau đó lưu vào cache.
                // ...
            }
        }
    }
}