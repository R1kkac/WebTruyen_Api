using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;

namespace TestWebApi_v1.Models.ViewModel.MangaView
{
	public class AddeditView
	{
		public string MangaId { get; set; } = null!;
		public string MangaName { get; set; } = null!;
		public string? MangaDetails { get; set; }
		public string? MangaImage { get; set; }
		public string? MangaAlternateName { get; set; }
		public string MangaAuthor { get; set; } 
		public string? MangaArtist { get; set; }
		public int? Type { get; set; }
		public string? Id { get; set; }
		public List<int> GenreIds { get; set; }
	}
	
}
