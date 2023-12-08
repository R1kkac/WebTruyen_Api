using TestWebApi_v1.Models.TruyenTranh.MangaView;

namespace TestWebApi_v1.Models.DbContext
{
    public class BotruyenViewCount
    {
        public string Id { get; set; } = null!;
        public int Viewbydate { get; set; }
        public int Viewbymonth { get; set; }
        public int Viewbyyear { get; set; }



        public virtual BoTruyen botruyen { get; set; } = null!;
    }
}
