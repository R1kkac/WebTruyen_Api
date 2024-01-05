namespace TestWebApi_v1.Models.ResponeViewModel.ResponeManga
{
	public class ImagePositionUpdateModel
	{
		public int ImageId { get; set; }
		public int NewPosition { get; set; }
	}
	public class ChapterImageModel
	{
		public int ImageId { get; set; }
		public string ImageName { get; set; }
		public string ImageUrl { get; set; }
		public int ImageIndex { get; set; } 
	}

}
