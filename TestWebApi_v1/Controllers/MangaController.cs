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
using TestWebApi_v1.Service.Respone;


namespace TestWebApi_v1.Controllers
{
    [Route("Truyen-tranh")]
    [ApiController]
    public class MangaController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private ILogger<MangaController> _logger;
        private readonly IMangaRepo _mangaModel;
        private readonly RealTimeService _tb2;
        private readonly UserManager<User> _userManager;
        public MangaController(IMangaRepo mangaModel, ILogger<MangaController> logger,
            IMemoryCache memorycache,UserManager<User> userManager,RealTimeService tb2)
        {
            _mangaModel = mangaModel;
            _logger = logger;
            _memoryCache = memorycache;
            _userManager = userManager;
            _tb2 = tb2;

        }
        //Bộ truyện
        [HttpGet("SearchManga")]
        public async Task<IEnumerable<Searchmanga>> searManga()
        {
            return await _mangaModel.search();
        }
        [HttpGet("MangaNewUpdate")]
        public async Task<List<ResponeManga>> getMangaUpdate() 
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
        [HttpGet("topmanga_by_type/{type}/{page}/{size}")]
        public async Task<ResultForTopView> getTOpManga(string type, string page, string size)
        {
            int typemanga = int.Parse(type);
            int pagenumber = int.Parse(page);
            int pagesize = int.Parse(size); ;
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result =await _mangaModel.getTopmanga(pagenumber, pagesize, typemanga, requestUrl);
            return result;
        }
		//Lấy tất cả truyện
		[HttpGet("GetAllManga")]
		public async Task<IActionResult> GetAllManga()
		{
			try
			{
				Stopwatch t = new Stopwatch();
				t.Start();
				var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
				string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
				string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";

				// Gọi hàm mới để lấy tất cả dữ liệu
				var result = await _mangaModel.LayTatCaTruyen(requestUrl, routeController);

				_memoryCache.Set("cachedData", result);
				t.Stop();
				Console.WriteLine("Eslaped Time QueryDatabase: " + t.Elapsed);
				return Json(result);
			}
			catch (Exception ex)
			{
				// Ghi log lỗi nếu cần
				Console.WriteLine("Error: " + ex.Message);
				return NotFound();
			}
		}
		//Lấy truyện theo page
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
		//Lấy truyện theo User
		[HttpGet("GetAllUserManga")]
		public async Task<IActionResult> GetAllUserManga()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			try
			{
				Stopwatch t = new Stopwatch();
				t.Start();
				var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
				string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
				string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";

				// Gọi hàm mới để lấy tất cả dữ liệu
				var result = await _mangaModel.LayTatCaTruyenTheoUserId(userId, requestUrl, routeController);

				_memoryCache.Set("cachedData", result);
				t.Stop();
				Console.WriteLine("Eslaped Time QueryDatabase: " + t.Elapsed);
				return Json(result);
			}
			catch (Exception ex)
			{
				// Ghi log lỗi nếu cần
				Console.WriteLine("Error: " + ex.Message);
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
		//Lấy tất cả tên truyện
		[HttpGet("GetAllNameManga")]
		public async Task<IActionResult> GetAllNameManga()
		{
			try
			{
				var danhSachTenTruyen = await _mangaModel.LayDanhSachTenTruyen();
				return Ok(danhSachTenTruyen);
			}
			catch (Exception ex)
			{
				// Ghi log lỗi nếu cần
				return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi lấy danh sách tên truyện: " + ex.Message);
			}
		}
		////Tạo bộ truyện
		[Authorize(Roles = "Admin,Upload")]
		[HttpPost("Create")]
		//[EnableCors("Policy")]
		public async Task<IActionResult> TaoTruyen([FromForm] AddeditView mangaDto, IFormFile? mangaImage)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID của người dùng hiện tại
			bool result = await _mangaModel.TaoTruyen(userId, mangaDto, mangaImage);

			if (result)
				return Ok();
			else
				return BadRequest("Không thể tạo truyện");
		}
		//Sửa thông tin của bộ truyện
		[Authorize(Roles = "Admin,Upload")]
		[HttpPut("EditManga/{MangaId}")]
		//[EnableCors("Policy")]
		public async Task<IActionResult> EditManga(string mangaId, [FromForm] AddeditView mangaDto, IFormFile? mangaImage)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID người dùng hiện tại
			if (userId == null)
			{
				return Unauthorized("User is not authorized");
			}

			bool result = await _mangaModel.SuaTruyen(userId, mangaDto, mangaImage);

			if (result)
			{
				return Ok("Manga updated successfully");
			}
			else
			{
				return BadRequest("Failed to update manga");
			}
		}
		//Cập nhập Status bộ truyện
		[Authorize(Roles = "Admin,Upload")]
		[HttpPut("Status/{mangaId}")]
		public async Task<IActionResult> UpdateStatus(string mangaId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (userId == null)
			{
				return Unauthorized("User is not authorized");
			}

			bool result = await _mangaModel.UpdateStatusAsync(mangaId, userId);

			if (result)
			{
				return Ok("Manga updated successfully");
			}

			else
			{
				return BadRequest("Failed to update manga");
			}
		}
		//Cập nhập DeleteStatus bộ truyện
		[Authorize(Roles = "Admin,Upload")]
		[HttpPut("DeleteStatus/{mangaId}")]
		public async Task<IActionResult> UpdateDeleteStatus(string mangaId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (userId == null)
			{
				return Unauthorized("User is not authorized");
			}

			bool result = await _mangaModel.DeleteStatus(mangaId, userId);

			if (result)
			{
				return Ok("Manga updated successfully");
			}

			else
			{
				return BadRequest("Failed to update manga");
			}
		}
		//Xóa bộ truyện
		[Authorize(Roles = "Admin,Upload")]
		[EnableCors("Policy")]
		[HttpDelete("Delete/{MangaId}")]
		public async Task<IActionResult> DeleteManga(string MangaId)
		{
			try
			{
				var Id = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
				var result = await _mangaModel.XoaTruyen(Id, MangaId);
				if (result)
				{
					return StatusCode(StatusCodes.Status200OK,
							new ResponeStatus { Status = "Success", Message = $"Xóa thành công" });
				}
				return StatusCode(StatusCodes.Status404NotFound,
							new ResponeStatus { Status = "Failed", Message = $"Xóa thất bại" });
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status404NotFound,
							new ResponeStatus { Status = "Error", Message = $"Gặp lỗi trong quá trình xóa" });
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
                            new ResponeStatus { Status = "Error", Message = $"Gặp lỗi trong quá trình lấy dữ liệu" });
            }
        }

		//Lấy tất cả tên chương
		[HttpGet("GetAllNameChapter/{MangaId}")]
		public async Task<IActionResult> GetAllNameChapter(string MangaId)
		{
			try
			{
				var danhSachTenTruyen = await _mangaModel.LayDanhSachTenChuong(MangaId);
				return Ok(danhSachTenTruyen);
			}
			catch (Exception ex)
			{
				// Ghi log lỗi nếu cần
				return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi khi lấy danh sách tên truyện: " + ex.Message);
			}
		}

		//Tạo chương truyện của bộ truyện
		[Authorize(Roles = "Admin,Upload")]
		[EnableCors("Policy")]
		[HttpPost("{MangaId}/CreateChapter")]
		public async Task<IActionResult> CreateChapter([FromForm] string Chapter, [FromForm] List<IFormFile> MangaImage, [FromForm] List<string> MangaUrl)
		{
			try
			{
				var result = await _mangaModel.TaoChuongTruyen(Chapter, MangaImage, MangaUrl);
				if (result)
				{
					return StatusCode(StatusCodes.Status200OK,
							new ResponeStatus { Status = "Success", Message = $"Thành công" });
				}
				return StatusCode(StatusCodes.Status404NotFound,
							new ResponeStatus { Status = "Failed", Message = $"Thất bại" });
			}
			catch (Exception)
			{
				return StatusCode(StatusCodes.Status404NotFound,
							new ResponeStatus { Status = "Error", Message = $"Đã xảy ra lỗi" });
			}
		}

		//Sửa chương truyện của bộ truyện
		[Authorize(Roles = "Admin,Upload")]
		[EnableCors("Policy")]
		[HttpPut("{MangaId}/{ChapterId}/EditChapter")]
		public async Task<IActionResult> editChapter([FromForm] string Chapter, [FromForm] List<IFormFile> MangaImage, [FromForm] List<string> MangaUrl)
		{
			try
			{
				var result = await _mangaModel.SuaChuongTruyen(Chapter, MangaImage, MangaUrl);
				if (result)
				{
					return Ok(new ResponeStatus { Status = "Success", Message = "Chỉnh sửa thành công" });
				}
				else
				{
					// Dữ liệu không tìm thấy hoặc không thể chỉnh sửa
					return NotFound(new ResponeStatus { Status = "Failed", Message = "Chương truyện không tìm thấy hoặc không thể chỉnh sửa" });
				}
			}
			catch (Exception ex)
			{
				// Lỗi server
				return StatusCode(StatusCodes.Status500InternalServerError,
						  new ResponeStatus { Status = "Error", Message = $"Đã xảy ra lỗi: {ex.Message}" });
			}
		}

		//Xóa chương truyện
		[Authorize(Roles = "Admin,Upload")]
		[EnableCors("Policy")]
		[HttpDelete("{MangaId}/{ChapterId}/DeleteChapter")]
		public async Task<IActionResult> deleteChapter(string MangaId, string ChapterId)
		{
			try
			{
				var result = await _mangaModel.XoaChuongTruyen(MangaId, ChapterId);
				if (result)
				{
					return Ok(new ResponeStatus { Status = "Success", Message = "Xóa thành công" });
				}
				else
				{
					// Nếu không tìm thấy chương để xóa
					return NotFound(new ResponeStatus { Status = "Not Found", Message = "Chương không tìm thấy để xóa" });
				}
			}
			catch (Exception ex)
			{
				// Trả về lỗi nội bộ của server
				return StatusCode(StatusCodes.Status500InternalServerError,
						  new ResponeStatus { Status = "Error", Message = $"Đã xảy ra lỗi: {ex.Message}" });
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
                            new ResponeStatus { Status = "Failed", Message = $"Truy xuất thất bại" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                            new ResponeStatus { Status = "Error", Message = $"Gặp lỗi trong quá trình lấy dữ liệu" });
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
                             new ResponeStatus { Status = "Failed", Message = $"Truy xuất thất bại" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status404NotFound,
                             new ResponeStatus { Status = "Error", Message = $"Gặp lỗi trong quá trình truy xuất" });
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
                              new ResponeStatus { Status = "Failed", Message = $"Not Found" });
        }
        //lấy danh sách manga theo thể loại
        [HttpGet("GetmangabyCategory/{idCategory}/{pagenumber}/{pagesize}")]
        public async Task<ResultForMangaView> getMangabyCategry(string idCategory, string pagenumber, string pagesize)
        {
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result = await _mangaModel.getMangaByCategory(idCategory, pagenumber, pagesize, requestUrl);
            return result;
        }

        //lấy danh sách thể loại
        [HttpGet("Category/Getall")]
		public async Task<List<ResponeCategory>> getCategories()
		{
			var result = await _mangaModel.getListCategory();
			return result;
		}
		//Thêm thể loại
		[Authorize(Roles = "Admin")]
		[EnableCors("Policy")]
		[HttpPost("CreateGenre")]
		public async Task<IActionResult> CreateGenre([FromBody] CategoryAddedit categoryAddedit)
		{
			bool result = await _mangaModel.AddTheLoai(categoryAddedit);

			if (result)
			{
				return Ok(new { message = "Genre created successfully." });
			}
			else
			{
				return BadRequest(new { message = "Failed to create genre." });
			}
		}
		//Xóa thể loại
		[Authorize(Roles = "Admin")]
		[EnableCors("Policy")]
		[HttpDelete("{GenreId}/DeleteGenre")]
		public async Task<IActionResult> deleteGenre(int GenreId)
		{
			try
			{
				var result = await _mangaModel.DeleteTheLoai(GenreId);
				if (result)
				{
					return Ok(new ResponeStatus { Status = "Success", Message = "Xóa thành công" });
				}
				else
				{
					// Phân biệt giữa không tìm thấy và không thể xóa do có liên kết
					return BadRequest(new ResponeStatus { Status = "Bad Request", Message = "Thể loại không thể xóa do có bộ truyện liên kết" });
				}
			}
			catch (Exception ex)
			{
				// Trả về lỗi nội bộ của server
				return StatusCode(StatusCodes.Status500InternalServerError,
						  new ResponeStatus { Status = "Error", Message = $"Đã xảy ra lỗi: {ex.Message}" });
			}
		}
		//Sửa thể loại
		[Authorize(Roles = "Admin")]
		[EnableCors("Policy")]
		[HttpPut("{GenreId}/UpdateGenre")]
		public async Task<IActionResult> UpdateGenre(int GenreId, [FromBody] CategoryAddedit categoryAddedit)
		{
			try
			{
				var result = await _mangaModel.UpdateTheLoai(GenreId, categoryAddedit);
				if (result)
				{
					return Ok(new ResponeStatus { Status = "Success", Message = "Cập nhật thể loại thành công" });
				}
				else
				{
					// Phân biệt giữa không tìm thấy và lỗi khác
					return BadRequest(new ResponeStatus { Status = "Bad Request", Message = "Không tìm thấy thể loại hoặc không thể cập nhật" });
				}
			}
			catch (Exception ex)
			{
				// Trả về lỗi nội bộ của server
				return StatusCode(StatusCodes.Status500InternalServerError,
						  new ResponeStatus { Status = "Error", Message = $"Đã xảy ra lỗi: {ex.Message}" });
			}
		}
		
        //lấy danh sách trang
        [HttpGet("GetPageNumber")]
        public async Task<int> getPageNumber()
        {
            return await _mangaModel.getPageNumber();
        }
        //Lấy manga theo danh sách thể loại
        [HttpGet("GetMangaByListCategories")]
        public async Task<List<ResponeManga>> GetMangaByCategories([FromQuery] List<string> List)
        {
            var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
            string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result =await _mangaModel.getMangaByCategories(List, requestUrl, routeController);
            return result; ;
        }
        //Lấy manga thỏa hết theo danh sách thể loại
        [HttpGet("get_manga_all_categories")]
        public async Task<string> GetMangaByCategoriesALl([FromQuery] List<string> List)
        {
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result = await _mangaModel.getMangaByCategoriesAll(List, requestUrl);
            return result; 
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
        [HttpGet]
        [Route("all_manga_by_type/{type}/{pagenumber}/{pagesize}")]
        public async Task<List<botruyenViewforTopmanga>> mangaByType(string type, string pagenumber, string pagesize)
        {
            int curtype = int.Parse(type);
            int curpagesize = int.Parse(pagesize);
            int curpagenumber = int.Parse(pagenumber);
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result =await _mangaModel.danhSahcBotruyen(curtype, curpagesize, curpagenumber, requestUrl);
            return result;
        }
        [HttpGet]
        [Route("number_all_manga")]
        public async Task<int> getNumberAllManga()
        {
            return await _mangaModel.numbermanga();
        }
        [HttpGet("LayThuCache")]
        public async Task<IEnumerable<ResponeManga>> getdata( )
        {
            if (_memoryCache.TryGetValue("cachedData", out List<ResponeManga> cachedData))
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
		//Lấy số lượng truyện đăng tải
		[HttpGet("PublishedStoryCount")]
		public async Task<IActionResult> GetPublishedStoryCount()
		{
			var count = await _mangaModel.GetDailyPublishedStoryCountAsync();
			return Ok(count);
		}
		//Lấy kiểu truyện
		[HttpGet("GetAllType")]
		public IActionResult GetAll()
		{
			var typeMangas = _mangaModel.GetAllTypeMangas();
			return Ok(typeMangas);
		}
	}
}