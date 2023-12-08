using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TestWebApi_v1.Repositories
{
    public interface IServices
    {
        string ranDomId();
        string OnGetFolderPath(string folder);
        bool CreateFolder(string id, string FolderPath);
        Task UpLoadimage(IFormFile imageFile, string filePath);
        void DeleteImage(string filePath);
        void DeleteAllFilesInFolder(string folderPath);
        void DeleteFolder(string folderPath);
        string getImageManga(string requestUrl, string conTrollerName, string idManga, string imageName);
        string getUrlImageforChapter(string requestUrl, string conTrollerName, string idManga, string idChapter, string imageName);
        Task<bool> checkUrlImage(string url);

        string LayUrlAnh(string requestUrl, string conTrollerName, string Url);
        string? LayAvatarUser(string avatar);
        JwtSecurityToken getToken(List<Claim> authClaims);
    }
}
