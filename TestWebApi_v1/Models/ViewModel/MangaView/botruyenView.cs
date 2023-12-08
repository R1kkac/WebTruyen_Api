using TestWebApi_v1.Models.DbContext;

namespace TestWebApi_v1.Models.TruyenTranh.MangaView
{
    public class botruyenView
    {
        public string MangaId { get; set; } = null!;
        public string? MangaName { get; set; }
        public string? MangaDetails { get; set; }
        public string? MangaImage { get; set; }
        public string? MangaAlternateName { get; set; }
        public string? MangaAuthor { get; set; }
        public string? MangaArtist { get; set; }
        public string? MangaGenre { get; set; }
        public string? Dateupdate { get; set; }
        public string? Rating { get; set; }
        public string? View { get; set; }
        public bool? Status { get; set; }
        public List<chapterView2>? ListChaper { get; set; }
        public List<TheLoai>? Listcategory { get; set; }
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
        public string? MangaGenre { get; set; }
        public string? Dateupdate { get; set; }
        public string? Rating { get; set; }
        public string? View { get; set; }
        public bool? Status { get; set; }
        public List<TheLoai>? Listcategory { get; set; }

        public string? chaptercount { get; set; }
        public string? mangaCount { get; set; }
    }
    public class TopManga
    {
        public string MangaId { get; set; } = null!;
        public string? MangaName { get; set; }
        public string? MangaImage { get; set; }
        public int? View { get; set; }
        public string? Typetop { get; set; } //=1 top ngày, =2 top tháng, =3 top năm


    }
}
