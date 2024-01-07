namespace TestWebApi_v1.Models.DbContext
{
	public class MangaArtist
	{
		public MangaArtist()
		{
			BoTruyens = new HashSet<BoTruyen>();
		}

		public int MangaArtistId { get; set; }
		public string Name { get; set; }
		public DateTime? Birthday { get; set; }
		public string? AlternateName { get; set; }
		public string? ArtistImage { get; set; }


		public virtual ICollection<BoTruyen> BoTruyens { get; set; }
	}
}
