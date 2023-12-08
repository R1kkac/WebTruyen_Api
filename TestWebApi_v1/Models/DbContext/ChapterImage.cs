using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class ChapterImage
    {
        public int ImageId { get; set; }
        public string ImageName { get; set; } = null!;
        public string? ImageUl { get; set; }
        public string ChapterId { get; set; } = null!;

        public virtual ChuongTruyen Chapter { get; set; } = null!;
    }
}
