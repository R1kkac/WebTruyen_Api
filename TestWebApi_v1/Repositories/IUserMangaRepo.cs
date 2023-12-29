using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel.MangaView;
using TestWebApi_v1.Models.ViewModel.UserView;
using TestWebApi_v1.Service.Respone;

namespace TestWebApi_v1.Repositories
{
    public interface IUserMangaRepo
    {
        //Theo doi
        Task<ResponeData> TheoDoiTruyen(string IdUser, string IdManga);
        ResponeData HuyTheoDoi(string IdUser, string IdManga);
        Task<List<MangaFollowing>> DanhSachTheoDoi(string idUser, string requestUrl);

        //Thong bao
        Task<IEnumerable<ThongbaoUser>> LayToanBoThongBao(string idUser);
        Task<IEnumerable<ResponeNotification>> LayThongBaoChuaDoc(string idUser, string requestUrl);
        Task<ResponeData> TaoThongBao(string idUser, string mesage);
        Task<ResponeData> DaXemThongBao(string idThongbao);

        //Truyen tranh
        Task<IEnumerable<ResponeManga>> DanhSachTruyenUserTao(string idUser);
        Task<IEnumerable<User>> DanhSachUserTheoRole(string idRole);
        Task DemViewBoTruyen(string idBotruyen);

        //Binh Luan
        Task<List<ResponeComment>> danhSachBinhluanCuaBoTruyen(string IdManga, int? page, int? number, string requesturl);
        Task<bool> BinhLuanChuongTruyen(string IdUser, string IdManga, string? IdChapter, string CommentData);
        Task<bool> ReplyBinhLuanChuong(string IdComment, string IdUser, string Replydata);
        Task<List<danhSachBinhLuan>> danhSachBinhLuanTheoChuuong(string mangaId, string idChuong, string requesturl);
        Task<List<danhSachReplyBinhLuan>> layDanhSachPhanHoi(string idComment);

        //Danh gia
        Task<bool> DanhgiaTruyen(string MangaId, string star);
        Task<bool> likeComment(string Idcomment);
        Task<bool> disLikeComment(string Idcomment);
        Task<bool> unlikeComment(string Idcomment);
        Task<bool> undisLikeComment(string Idcomment);
        Task<int> numberComment(string mangaId);



    }
}
