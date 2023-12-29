using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel.MangaView;
using TestWebApi_v1.Models.ViewModel.UserView;
using TestWebApi_v1.Repositories;
using TestWebApi_v1.Service;
using TestWebApi_v1.Service.Respone;

namespace TestWebApi_v1.Controllers
{
    [Route("Services")]
    [ApiController]
    public class ServiceController : Controller
    {
        private readonly IUserMangaRepo _userMangaModel;
        private readonly IMapper _mapper;
        public ServiceController(IUserMangaRepo _Model, IMapper mapper)
        {
            _userMangaModel = _Model;
            _mapper = mapper;
        }
        [HttpPost]
        [Route("TheoDoiTruyen")]
        public async Task<IActionResult> Follow([FromForm] string IdUser, [FromForm] string IdManga)
        {
            var result = await _userMangaModel.TheoDoiTruyen(IdUser, IdManga);
            if (result.Value == true)
            {
                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = result.Message });
            }
            return StatusCode(StatusCodes.Status403Forbidden,
                    new ResponeStatus { Status = "Failed", Message = result.Message });
        }
        [HttpDelete]
        [Route("HuyTheoDoi/{IdUser}/{IdManga}")]
        public IActionResult UnFollow(string IdUser, string IdManga)
        {
            var result = _userMangaModel.HuyTheoDoi(IdUser, IdManga);
            if (result.Value == true)
            {
                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = result.Message });
            }
            return StatusCode(StatusCodes.Status403Forbidden,
                    new ResponeStatus { Status = "Failed", Message = result.Message });
        }
        [HttpGet]
        [Route("DanhsachTheoDoi/{idUser}")]
        public async Task<List<MangaFollowing>> ListFollowing(string idUser)
        {
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result = await _userMangaModel.DanhSachTheoDoi(idUser, requestUrl);
            //var data = _mapper.Map<List<BookmarkView>>(result);
            return result;
        }
        [HttpGet]
        [Route("DanhSachThongBao")]
        public async Task<IEnumerable<ThongbaoUser>> ListNotification(string idUser)
        {
            var result = await _userMangaModel.LayToanBoThongBao(idUser);
            return result;
        }
        [HttpGet]
        [Route("DanhSachThongBaoChuaXem/{idUser}")]
        public async Task<IEnumerable<ResponeNotification>> ListNotificationUnSeen(string idUser)
        {
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result = await _userMangaModel.LayThongBaoChuaDoc(idUser, requestUrl);
            return result;
        }
        [HttpPost]
        [Route("ThemThongBao")]
        public async Task<IActionResult> ThemThongbao(string idUser, string message)
        {
            var result = await _userMangaModel.TaoThongBao(idUser, message);
            if (result.Value == true)
            {
                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = result.Message });
            }
            return StatusCode(StatusCodes.Status403Forbidden,
                new ResponeStatus { Status = "Failed", Message = result.Message });
        }
        [HttpPost]
        //Khi người dùng nhấp vào thông báo ở web client thì sẽ gửi request đến đây để đánh dấu là đã xem
        [Route("SeenNotification")]
        public async Task<bool> seenNotificaton([FromForm] string idNotification)
        {
            var result = await _userMangaModel.DaXemThongBao(idNotification);
            if (result.Value == true)
            {
                return true;
            }
            return false;
        }
        [HttpGet]
        [Route("DanhSachTruyen")]
        public async Task<IEnumerable<ResponeManga>> ListMangaOfUser(string idUser)
        {
            var result = await _userMangaModel.DanhSachTruyenUserTao(idUser);
            return result;
        }
        [HttpGet]
        [Route("DanhSachUserTheoRole")]
        public async Task<IEnumerable<User>> ListUserOfRole(string idUser)
        {
            var result = await _userMangaModel.DanhSachUserTheoRole(idUser);
            return result;
        }
        [HttpPost]
        [Route("CapNhatView/{MangaId}")]
        public async Task<IActionResult> UpdateViewManga([FromForm] string MangaId)
        {
            await _userMangaModel.DemViewBoTruyen(MangaId);
            return Ok();
        }
        //Danh sách bình luận theo chương
        [HttpGet]
        [Route("GetListComment/{mangaId}/{ChapterId}")]
        public async Task<List<danhSachBinhLuan>> GetListCommentByChapter(string mangaId,string ChapterId)
        {
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result =await _userMangaModel.danhSachBinhLuanTheoChuuong(mangaId,ChapterId, requestUrl);
            if (result != null)
            {
                return result;
            }
            return new List<danhSachBinhLuan>() { };
        }
        //Danh sách bình luận theo bộ truyện
        [HttpGet]
        [Route("manga_comment_manga/{MangaId}/{PageSize}/{PageNumber}")]
        public async Task<List<ResponeComment>> GetCommentForManga(string MangaId, string PageSize, string PageNumber)
        {
            int size = int.Parse(PageSize);
            int number = int.Parse(PageNumber);
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result = await _userMangaModel.danhSachBinhluanCuaBoTruyen(MangaId, size, number, requestUrl);
            return result;
        }
        //Danh sách phản hồi bình luận
        [HttpGet]
        [Route("ListReply/{IdComment}")]
        public async Task<List<danhSachReplyBinhLuan>> GetListReply(string IdComment)
        {
            var result = await _userMangaModel.layDanhSachPhanHoi(IdComment);
            if (result != null)
            {
                return result;
            }
            return new List<danhSachReplyBinhLuan>() { };
        }
        //Bình luận
        [HttpPost]
        [Route("Comment")]
        public async Task<bool> Comment([FromForm] string IdUser, [FromForm] string IdManga, [FromForm] string? IdChapter, [FromForm] string CommentData)
        {
            var result = await _userMangaModel.BinhLuanChuongTruyen(IdUser, IdManga, IdChapter, CommentData);
            return result;
        }
        // Phản hồi bình luận
        [HttpPost]
        [Route("ReplyComment")]
        public async Task<bool> ReplyCommentChapter([FromForm] string IdComment, [FromForm] string IdUserReply, [FromForm] string ReplyData)
        {
            var result = await _userMangaModel.ReplyBinhLuanChuong(IdComment, IdUserReply, ReplyData);
            return result;
        }
        //Đánh giá truyện
        [HttpPost]
        [Route("rating")]
        public async Task<bool> RatingManga([FromForm] string MangaId, [FromForm] string star)
        {
            var result=await _userMangaModel.DanhgiaTruyen(MangaId, star);
            return result;
        }

        [HttpPost]
        [Route("like_comment")]
        public async Task<bool> likeComment([FromForm] string idcomment)
        {
            var result = await _userMangaModel.likeComment(idcomment);
            return result;
        }
        [HttpPost]
        [Route("dislike_comment")]
        public async Task<bool> dislikeComment([FromForm] string idcomment)
        {
            var result = await _userMangaModel.disLikeComment(idcomment);
            return result;
        }
        [HttpPost]
        [Route("un_like_comment")]
        public async Task<bool> unlikeComment([FromForm] string idcomment)
        {
            var result = await _userMangaModel.likeComment(idcomment);
            return result;
        }
        [HttpPost]
        [Route("un_dislike_comment")]
        public async Task<bool> undislikeComment([FromForm] string idcomment)
        {
            var result = await _userMangaModel.disLikeComment(idcomment);
            return result;
        }
        [HttpGet]
        [Route("comment_count/{mangaid}")]
        public async Task<int> numbercomment(string mangaid)
        {
            var result = await _userMangaModel.numberComment(mangaid);
            return result;
        }
    }
}
