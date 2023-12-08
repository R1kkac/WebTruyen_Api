using System;
using System.Collections.Generic;

namespace TestWebApi_v1.Models.DbContext
{
    public partial class TypeManga
    {
        public TypeManga()
        {
            Mangas = new HashSet<BoTruyen>();
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        public virtual ICollection<BoTruyen> Mangas { get; set; }
    }
}
