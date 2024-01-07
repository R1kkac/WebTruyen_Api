using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Repositories;

namespace TestWebApi_v1.Models.ViewModel.MangaView
{
    public class ResponeMangaInfo: BoTruyen
    {
        public string? requesturl { get; set; }
        public string? routecontroller { get; set; }
    }
	public class ResponeArtistInfo : MangaArtist
	{
		public string? requesturl { get; set; }
		public string? routecontroller { get; set; }
	}
	public class ResponeAuthorInfo : MangaAuthor
	{
		public string? requesturl { get; set; }
		public string? routecontroller { get; set; }
	}
}
