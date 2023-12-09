using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel.MangaView;
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
        Task<IEnumerable<ThongbaoUser>> LayThongBaoChuaDoc(string idUser);
        Task<ResultService> TaoThongBao(string idUser, string mesage);
        Task<ResultService> DaXemThongBao(string idThongbao);

        //Truyen tranh
        Task<IEnumerable<botruyenView>> DanhSachTruyenUserTao(string idUser);
        Task<IEnumerable<User>> DanhSachUserTheoRole(string idRole);
        Task<ResultService> DemViewBoTruyen(string idBotruyen, string View);

        //Binh Luan
        Task<bool> BinhLuanChuongTruyen(string IdUser, string IdChapter, string CommentData);
        Task<bool> ReplyBinhLuanChuong(string IdComment, string IdUser, string Replydata);
        Task<List<danhSachBinhLuan>> danhSachBinhLuanTheoChuuong(string idChuong);
        Task<List<danhSachReplyBinhLuan>> layDanhSachPhanHoi(string idComment);


    }
}
