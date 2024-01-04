
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TestWebApi_v1.Models.Account;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.Roles;
using TestWebApi_v1.Models.ViewModel.UserView;
using TestWebApi_v1.Service.Respone;

namespace TestWebApi_v1.Repositories
{
    public interface IUserRepo
    {
        Task<UserInfo?> layThongTinNguoiDung(string idUser);
		Task<IEnumerable<ResponeRole>> GetUserRolesAsync(string userId);
		Task<ResponeRegister> DangKytaiKhoan(string User);
        Task<ResponeData> SuaThongTinTaiKhoan(string User, IFormFile? Avatar);
        Task<ResponeData> XoataiKhoan(string email);
        Task<bool> thayDoiMatKhau(string userchangepassword);

        Task<ResponeData> ThemRoleUser(string idUser, string role);
        Task<ResponeData> XoaRoleUser(string idUser, string role);

        Task<ResponeData> TaoRoleMoi(Models.Roles.RoleRequest role);
        Task<ResponeData> ChinhSuaRole(string idRole, string roleName);
        Task<ResponeData> XoaRole(string idRole);
        public JwtSecurityToken getToken(List<Claim> authClaims);
    }
}
