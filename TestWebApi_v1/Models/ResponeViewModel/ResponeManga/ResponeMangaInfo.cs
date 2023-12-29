using TestWebApi_v1.Models.DbContext;

namespace TestWebApi_v1.Models.ViewModel.MangaView
{
    public class ResponeMangaInfo: BoTruyen
    {
        public string? requesturl { get; set; }
        public string? routecontroller { get; set; }
    }
}
