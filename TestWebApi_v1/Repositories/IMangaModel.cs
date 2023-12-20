using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel.MangaView;

namespace TestWebApi_v1.Repositories
{
    public interface IMangaModel
    {
        //Botruyen
        Task<IEnumerable<Searchmanga>> responeSearch(string value, string url);
        Task<IEnumerable<Searchmanga>> search();
        Task<IEnumerable<botruyenView>> LayDanhSachTruyenTheoPage(int? page, string requestUrl, string routeController);
        Task<botruyenView?> LayThongTinTruyen(string MangaId, string requestUrl, string routeController);
        Task<List<botruyenViewforTopmanga>> danhSahcBotruyen(int type, int pagenumber, int pagesize, string requesurl);
        string? LayAnhTruyen(string imageManga);
        Task<bool> TaoTruyen(string idUser, string Manga, IFormFile? MangaImage);
        Task<bool> SuaTruyen(string Id, string Manga, IFormFile? MangaImage);
        Task<bool> XoaTruyen(string iduUser, string MangaId);
        Task<List<botruyenView>> getMangaNewUpdate(string requestUrl, string routeController);
        Task<int> getPageNumber();
        Task<List<botruyenViewforTopmanga>> getTopmanga(int page, int number, int type, string requestUrl);
        Task<List<botruyenView>> getMangaByCategories(List<string> listCategories, string requestUrl, string routeController);
        Task<List<botruyenView>> getMangaByCategoriesAll(List<string> listCategories, string requestUrl);
        Task<List<TopManga>> Topmangabydate(int? page, string requestUrl, string routeController);
        Task<List<TopManga>> Topmangabymonth(int? page, string requestUrl, string routeController);
        Task<List<TopManga>> TopmangabyYear(int? page, string requestUrl, string routeController);
        Task<List<TopManga>> getTopMangaDefault(string requestUrl, string routeController);
        Task<int> numbermanga();






        //Chuongttuyen
        Task<IEnumerable<ChuongTruyen>> LayTatcaChuongTruyen();
        Task<bool> TaoChuongTruyen(string Chapter, List<IFormFile> MangaImage, List<string> MangaUrl);
        Task<bool> SuaChuongTruyen(string Chapter, List<IFormFile> MangaImage, List<string> MangaUrl);
        Task<bool> XoaChuongTruyen(string MangaId, string ChapterId);
        Task<IEnumerable<chapterView2>> DanhSachChuongCuaBoTruyen(string idManga, string requestUrl, string routeController);
        Task<IEnumerable<string>> DanhSachAnhTheoChuong(string idManga, string idChapter, string requestUrl, string routeController);
        string LayAnh(string idManga, string idChapter, string image);
        //Theloai
        Task<List<CategoryView>> getListCategory();
        Task<List<botruyenView>> getMangaByCategory(string id, string requestUrl, string routeController);

    }
}
