namespace TestWebApi_v1.Models.ViewModel;

public class EditManga
{
    public string MangaId { get; set; } = null!;
    public string? MangaName { get; set; }
    public string? MangaDetails { get; set; }
    public string? MangaImage { get; set; }
    public string? MangaAlternateName { get; set; }
    public string? MangaAuthor { get; set; }
    public string? MangaArtist { get; set; }
    public string? MangaGenre { get; set; }
    public string? Id { get; set; }
}
