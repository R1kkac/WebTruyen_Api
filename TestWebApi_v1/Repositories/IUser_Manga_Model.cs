using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel.MangaView;
using TestWebApi_v1.Models.ViewModel.UserView;
using TestWebApi_v1.Service;

namespace TestWebApi_v1.Repositories
{
    public interface IUser_Manga_Model
    {
        //Theo doi
        Task<ResultService> TheoDoiTruyen(string IdUser, string IdManga);
        ResultService HuyTheoDoi(string IdUser, string IdManga);
        Task<List<MangaFollowing>> DanhSachTheoDoi(string idUser, string requestUrl);

        //Thong bao
        Task<IEnumerable<ThongbaoUser>> LayToanBoThongBao(string idUser);
        Task<IEnumerable<NotificationView>> LayThongBaoChuaDoc(string idUser, string requestUrl);
        Task<ResultService> TaoThongBao(string idUser, string mesage);
        Task<ResultService> DaXemThongBao(string idThongbao);

        //Truyen tranh
        Task<IEnumerable<botruyenView>> DanhSachTruyenUserTao(string idUser);
        Task<IEnumerable<User>> DanhSachUserTheoRole(string idRole);
        Task DemViewBoTruyen(string idBotruyen);

        //Binh Luan
        Task<List<CommentViewModel>> danhSachBinhluanCuaBoTruyen(string IdManga, int? page, int? number, string requesturl);
        Task<bool> BinhLuanChuongTruyen(string IdUser, string IdManga, string? IdChapter, string CommentData);
        Task<bool> ReplyBinhLuanChuong(string IdComment, string IdUser, string Replydata);
        Task<List<danhSachBinhLuan>> danhSachBinhLuanTheoChuuong(string idChuong);
        Task<List<danhSachReplyBinhLuan>> layDanhSachPhanHoi(string idComment);

        //Danh gia
        Task<bool> DanhgiaTruyen(string MangaId, string star);



    }
}
