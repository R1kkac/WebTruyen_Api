namespace TestWebApi_v1.Models.ResponeViewModel.ResponeManga
{
	public class ResponeAuthor
	{
		public int? MangaAuthorId { get; set; }
		public string? Name { get; set; }
		public DateTime? Birthday { get; set; }
		public string? AlternateName { get; set; }
		public string? AuthorImage { get; set; }
	}
	public class AuthorAddedit
	{
		public string? Name { get; set; }
		public DateTime? Birthday { get; set; }
		public string? AlternateName { get; set; }
		public string? AuthorImage { get; set; }
	}
}
