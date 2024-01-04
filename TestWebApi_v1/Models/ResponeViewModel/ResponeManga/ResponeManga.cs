using TestWebApi_v1.Models.DbContext;

namespace TestWebApi_v1.Models.TruyenTranh.MangaView
{
    public class ResponeManga
    {
        public string MangaId { get; set; } = null!;
        public string? MangaName { get; set; }
        public string? MangaDetails { get; set; }
        public string? MangaImage { get; set; }
        public string? MangaAlternateName { get; set; }
        public string? MangaAuthor { get; set; }
        public string? MangaArtist { get; set; }
        public int? Type { get; set; }
        public string? Dateupdate { get; set; }
        public string? Rating { get; set; }
        public string? View { get; set; }
        public bool? Status { get; set; }
        public List<chapterView2>? ListChaper { get; set; }
        public List<TheLoai>? Listcategory { get; set; }
        public int? Comment { get; set; }

    }
    public class botruyenViewforTopmanga
    {
        public string MangaId { get; set; } = null!;
        public string? MangaName { get; set; }
        public string? MangaDetails { get; set; }
        public string? MangaImage { get; set; }
        public string? MangaAlternateName { get; set; }
        public string? MangaAuthor { get; set; }
        public string? MangaArtist { get; set; }
        public string? TypeManga { get; set; }
        public string? Dateupdate { get; set; }
        public string? Rating { get; set; }
        public string? View { get; set; }
        public int? numberFollow { get; set; }
        public bool? Status { get; set; }
        public List<TheLoai>? Listcategory { get; set; }

        public string? chaptercount { get; set; }
        public string? mangaCount { get; set; }
    }
    public class ResultForTopView
    {
        public int numberManga { get; set; }
        public List<botruyenViewforTopmanga> listmanga { get; set; } = null!;
    }
    public class ResultForMangaView
    {
        public int numberManga { get; set; }
        public List<ResponeManga> listmanga { get; set; } = null!;
    }
    public class TopManga
    {
        public string MangaId { get; set; } = null!;
        public string? MangaName { get; set; }
        public string? MangaImage { get; set; }
        public int? View { get; set; }
        public string? Typetop { get; set; } //=1 top ngày, =2 top tháng, =3 top năm
    }
    public class MangaFollowing
    {
        public string MangaId { get; set; } = null!;
        public string? MangaName { get; set; }
        public string? MangaImage { get; set; }
        public string? MangaDetails { get; set; }
        public string? Dateupdate { get; set; }
    }
	public class CRUDView
	{
		public string MangaId { get; set; } = null!;
		public string? MangaName { get; set; }
		public string? MangaDetails { get; set; }
		public string? MangaImage { get; set; }
		public string? MangaAlternateName { get; set; }
		public string? MangaAuthor { get; set; }
		public string? MangaArtist { get; set; }
		public int? Type { get; set; }
		public string? Dateupdate { get; set; }
		public string? Rating { get; set; }
		public string? View { get; set; }
		public bool? Status { get; set; }
		public bool? DeleteStatus { get; set; }
		public List<chapterView2>? ListChaper { get; set; }
		public List<TheLoai>? Listcategory { get; set; }
		public int? Comment { get; set; }
	}
	public class AddeditView
	{
		public string MangaId { get; set; } = null!;
		public string MangaName { get; set; } = null!;
		public string? MangaDetails { get; set; }
		public string? MangaImage { get; set; }
		public string? MangaAlternateName { get; set; }
		public string MangaAuthor { get; set; }
		public string? MangaArtist { get; set; }
		public int? Type { get; set; }
		public string? Id { get; set; }
		public List<int> GenreIds { get; set; }
	}
}
