
namespace TestWebApi_v1.Models.DbContext
{
    public class Bookmark
    {
        public string IdUser { get; set; } = null!;
        public string IdBotruyen { get; set; }=null!;
        public virtual User User { get; set; } = null!;
        public virtual BoTruyen BoTruyen { get; set; } = null!;
    }
}
