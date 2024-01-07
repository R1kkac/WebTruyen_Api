using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.ResponeViewModel.ResponeManga;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel.MangaView;

namespace TestWebApi_v1.Repositories
{
    public interface IMangaRepo
    {
        //Botruyen
        Task<IEnumerable<Searchmanga>> responeSearch(string value, string url);
        Task<IEnumerable<Searchmanga>> search();
		Task<IEnumerable<CRUDView>> LayTatCaTruyen(string requestUrl, string routeController);
		Task<IEnumerable<ResponeManga>> LayDanhSachTruyenTheoPage(int? page, string requestUrl, string routeController);
		Task<IEnumerable<CRUDView>> LayTatCaTruyenTheoUserId(string userId, string requestUrl, string routeController);
		Task<ResponeManga?> LayThongTinTruyen(string MangaId, string requestUrl, string routeController);
        Task<List<botruyenViewforTopmanga>> danhSahcBotruyen(int type, int pagenumber, int pagesize, string requesurl);
        string? LayAnhTruyen(string imageManga);
		Task<IEnumerable<string>> LayDanhSachTenTruyen();
		Task<bool> TaoTruyen(string idUser, AddeditView mangaDto, IFormFile? MangaImage);
		Task<bool> SuaTruyen(string idUser, AddeditView mangaDto, IFormFile? MangaImage);
		Task<bool> UpdateStatusAsync(string mangaId, string idUser);
		Task<bool> DeleteStatus(string mangaId, string idUser);
		Task<bool> XoaTruyen(string iduUser, string MangaId);
		Task<List<ResponeManga>> getMangaNewUpdate(string requestUrl, string routeController);
        Task<int> getPageNumber();
        Task<ResultForTopView> getTopmanga(int page, int number, int type, string requestUrl);
        Task<List<ResponeManga>> getMangaByCategories(List<string> listCategories, string requestUrl, string routeController);
        Task<string> getMangaByCategoriesAll(List<string> listCategories, string requestUrl);
        Task<List<TopManga>> Topmangabydate(int? page, string requestUrl, string routeController);
        Task<List<TopManga>> Topmangabymonth(int? page, string requestUrl, string routeController);
        Task<List<TopManga>> TopmangabyYear(int? page, string requestUrl, string routeController);
        Task<List<TopManga>> getTopMangaDefault(string requestUrl, string routeController);
        Task<int> numbermanga();
		Task<Dictionary<DateTime, int>> GetDailyPublishedStoryCountAsync();






		//Chuongttuyen
		Task<IEnumerable<ChuongTruyen>> LayTatcaChuongTruyen();
		Task<IEnumerable<string>> LayDanhSachTenChuong(string MangaId);
		Task<bool> TaoChuongTruyen(string Chapter, List<IFormFile> MangaImage, List<string> MangaUrl);
		Task<bool> SuaChuongTruyen(string Chapter, List<IFormFile> MangaImage, List<string> MangaUrl);
		Task<bool> XoaChuongTruyen(string MangaId, string ChapterId);
		Task<IEnumerable<chapterView2>> DanhSachChuongCuaBoTruyen(string idManga, string requestUrl, string routeController);
		Task<chapterView2> ThongTinChuongTruyen(string idManga, string chapterId, string requestUrl, string routeController);

		Task<IEnumerable<string>> DanhSachAnhTheoChuong(string idManga, string idChapter, string requestUrl, string routeController);
		Task<IEnumerable<ChapterImageModel>> GetAllImageInChapter(string idManga, string idChapter, string requestUrl, string routeController);
		Task<bool> UploadChapterImages(string Chapter, List<IFormFile> MangaImage, List<string> MangaUrl);
		Task<bool> DeleteChapterImage(string mangaId, string chapterId, int imageId);
		Task UpdateImagePositions(string mangaId, string chapterId, List<ImagePositionUpdateModel> imageUpdates);

		string LayAnh(string idManga, string idChapter, string image);
        //Theloai
        Task<List<ResponeCategory>> getListCategory();
		Task<bool> AddTheLoai(CategoryAddedit categoryAddedit);
		Task<bool> DeleteTheLoai(int genreId);
		Task<bool> UpdateTheLoai(int genreId, CategoryAddedit categoryAddedit);
		Task<ResultForMangaView> getMangaByCategory(string id,string pagenumber, string pagesize, string requestUrl);

		//Kieutruyen
		IEnumerable<TypeManga> GetAllTypeMangas();
		Task<bool> AddTypeManga(ResponeType typeMangaDTO);
		Task<bool> DeleteTypeManga(int id);
		Task<bool> UpdateTypeManga(ResponeType typeMangaDTO);


		//Artist
		Task<List<ResponeArtist>> getListArtist();
		Task<bool> AddArtist(ArtistAddedit artistAddedit, IFormFile? ArtistImage);
		string LayHinhArtist(string image);
		Task<bool> DeleteArtist(int MangaArtistId);
		Task<bool> UpdateArtist(int MangaArtistId, ArtistAddedit artistAddedit, IFormFile? ArtistImage);

		//Author
		Task<List<ResponeAuthor>> getListAuthor();
		string LayHinhAuthor(string image);
		Task<bool> AddAuthor(AuthorAddedit authorAddedit, IFormFile? AuthorImage);
		Task<bool> DeleteAuthor(int MangaAuthorId);
		Task<bool> UpdateAuthor(int MangaAuthorId, AuthorAddedit authorAddedit, IFormFile? AuthorImage);
	}
}
