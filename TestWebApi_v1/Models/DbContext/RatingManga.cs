using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class RatingManga
    {
        public int Id { get; set; }
        public string Mangaid { get; set; } = null!;
        public double Rating { get; set; }
        public int NumberRating { get; set; }

        public virtual BoTruyen Manga { get; set; } = null!;
    }
}
