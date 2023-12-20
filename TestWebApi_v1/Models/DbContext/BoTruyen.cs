using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class BoTruyen
    {
        public BoTruyen()
        {
            ChuongTruyens = new HashSet<ChuongTruyen>();
            Genres = new HashSet<TheLoai>();
            BotruyenViewCounts = new HashSet<BotruyenViewCount>();
            bookmarks=new HashSet<Bookmark>();
            RatingMangas = new HashSet<RatingManga>();
            BinhLuans = new HashSet<BinhLuan>();

        }

        public string MangaId { get; set; } = null!;
        public string MangaName { get; set; } = null!;
        public string? MangaDetails { get; set; }
        public string? MangaImage { get; set; }
        public string? MangaAlternateName { get; set; }
        public string MangaAuthor { get; set; } = null!;
        public string? MangaArtist { get; set; }
        public int? Type { get; set; }
        public string? Id { get; set; }
        public DateTime? Dateupdate { get; set; }
        public bool? Status { get; set; }



        public virtual User? IdNavigation { get; set; }
        public virtual ICollection<ChuongTruyen> ChuongTruyens { get; set; }
        public virtual ICollection<BotruyenViewCount> BotruyenViewCounts { get; set; }
        public virtual ICollection<TheLoai> Genres { get; set; }
        public virtual ICollection<Bookmark> bookmarks { get; set; }
        public virtual ICollection<RatingManga> RatingMangas { get; set; }
        public virtual TypeManga? TypeNavigation { get; set; }
        public virtual ICollection<BinhLuan> BinhLuans { get; set; }


    }
    public class BoTruyenTopView
    {
        public string MangaId { get; set; } = null!;
        public string MangaName { get; set; } = null!;
        public string? MangaImage { get; set; }
        public int? View { get; set; }
        public string? requesturl { get; set; }
        public string? routecontroller { get; set; }
    }
}
