using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class ChuongTruyen
    {
        public ChuongTruyen()
        {
            ChapterImages = new HashSet<ChapterImage>();
            BinhLuans= new HashSet<BinhLuan>();
        }

        public string ChapterId { get; set; } = null!;
        public string ChapterName { get; set; } = null!;
        public string? ChapterTitle { get; set; }
        public DateTime ChapterDate { get; set; }
        public string MangaId { get; set; } = null!;
        public int? ChapterIndex { get; set; }


        public virtual BoTruyen Manga { get; set; } = null!;
        public virtual ICollection<ChapterImage> ChapterImages { get; set; }
        public virtual ICollection<BinhLuan> BinhLuans { get; set; }
    }
}
