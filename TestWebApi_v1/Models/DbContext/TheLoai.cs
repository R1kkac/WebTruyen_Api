using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class TheLoai
    {
        public TheLoai()
        {
            Mangas = new HashSet<BoTruyen>();
        }

        public int GenreId { get; set; }
        public string GenresIdName { get; set; } = null!;
        public string? Info { get; set; }


        public virtual ICollection<BoTruyen> Mangas { get; set; }
    }
}
