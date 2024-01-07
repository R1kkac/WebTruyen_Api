namespace TestWebApi_v1.Models.DbContext
{
	public class MangaAuthor
	{
		public MangaAuthor()
		{
			BoTruyens = new HashSet<BoTruyen>();
		}

		public int MangaAuthorId { get; set; }
		public string Name { get; set; }
		public DateTime? Birthday { get; set; }
		public string? AlternateName { get; set; }
		public string? AuthorImage { get; set; }

		public virtual ICollection<BoTruyen> BoTruyens { get; set; }
	}
}
