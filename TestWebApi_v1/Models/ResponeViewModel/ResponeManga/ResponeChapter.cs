using System.Diagnostics.CodeAnalysis;

namespace TestWebApi_v1.Models.TruyenTranh.MangaView
{
    public class ResponeChapter
    {
        public string? Chapter_Id { get; set; } = null!;
        public string? Chapter_Name { get; set; } = null!;
        public string? Chapter_Title { get; set; }
        public List<string>? Imagechapter { get; set; } = null!;
        public string Chapter_Date { get; set; } = null!;
        public string? Manga_Id { get; set; } = null!;
        public int? ChapterIndex { get; set; }
    }
    public class chapterView2
    {
        public string ChapterId { get; set; } = null!;
        public string? ChapterName { get; set; } = null!;
        public string? ChapterTitle { get; set; }
        public string? ChapterDate { get; set; }
        public int? ChapterIndex { get; set; }

    }
}
