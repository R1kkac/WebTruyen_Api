using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TestWebApi_v1.Repositories
{
    public class ServiceRepo:IServiceRepo
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        public ServiceRepo(IWebHostEnvironment evn, IConfiguration configuration) 
        {
            _env = evn;
            _configuration = configuration;
        }
        public string ranDomId()
        {
            Random data = new Random();
            int number = data.Next(1, 1000000);
            string randomNumber = number.ToString("d6");
            return randomNumber;
        }
        public string OnGetFolderPath(string folder)
        {
            var filePath = Path.Combine(
                    _env.ContentRootPath, folder);
            return filePath;
        }
        public bool CreateFolder(string id, string FolderPath)
        {
            string path = Path.Combine(FolderPath, id);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task UpLoadimage(IFormFile imageFile, string filePath)
        {
            using (var stream = new FileStream(filePath + "\\" + imageFile.FileName, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
                stream.Close();
            }
        }
        public void DeleteImage(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        public void DeleteAllFilesInFolder(string folderPath)
        {
            // Lấy danh sách tên tệp tin trong thư mục
            string[] fileNames = Directory.GetFiles(folderPath);

            // Xóa từng tệp tin
            foreach (string fileName in fileNames)
            {
                File.Delete(fileName);
            }
        }
        public void DeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }
        public string getImageManga(string requestUrl, string conTrollerName, string idManga, string imageName)
        {
            string url = requestUrl + conTrollerName + "/" + idManga + "/" + imageName;
            return url;
        }
        public string getUrlImageforChapter(string requestUrl, string conTrollerName, string idManga, string idChapter, string imageName)
        {
            string url = requestUrl + conTrollerName + "/" + idManga + "/" + idChapter + "/" + imageName;
            return url;
        }
        public async Task<bool> checkUrlImage(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }
                catch (HttpRequestException)
                {
                    return false;
                }
            }
        }

        public string LayUrlAnh(string requestUrl, string conTrollerName, string Url)
        {
            string url = requestUrl + conTrollerName + "/" + Url;
            return url;
        }
        public string? LayAvatarUser(string avatar)
        {
            // Đường dẫn đến thư mục chứa hình ảnh "Manga"
            string imagePath = Path.Combine(_env.ContentRootPath, "Manga", "Avatar", avatar);

            // Kiểm tra xem hình ảnh có tồn tại không
            if (System.IO.File.Exists(imagePath))
            {
                return imagePath;
            }
            return null;
        }
		public string? LayHinhArtist(string avatar)
		{
			// Đường dẫn đến thư mục chứa hình ảnh "Manga"
			string imagePath = Path.Combine(_env.ContentRootPath, "Manga", "ArtistImage", avatar);

			// Kiểm tra xem hình ảnh có tồn tại không
			if (System.IO.File.Exists(imagePath))
			{
				return imagePath;
			}
			return null;
		}

		public JwtSecurityToken getToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddMinutes(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }
    }
}
