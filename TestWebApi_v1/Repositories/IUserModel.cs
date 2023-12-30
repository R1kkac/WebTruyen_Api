
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TestWebApi_v1.Models.Account;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.Roles;
using TestWebApi_v1.Models.ViewModel.UserView;
using TestWebApi_v1.Service;

namespace TestWebApi_v1.Repositories
{
    public interface IUserModel
    {
        Task<UserInfo?> layThongTinNguoiDung(string idUser);
		Task<IEnumerable<RoleViewModel>> GetUserRolesAsync(string userId);

		Task<ResponeRegister> DangKytaiKhoan(string User);
        Task<ResultService> SuaThongTinTaiKhoan(string User, IFormFile? Avatar);
        Task<ResultService> XoataiKhoan(string email);

        Task<ResultService> ThemRoleUser(string idUser, string role);
        Task<ResultService> XoaRoleUser(string idUser, string role);

        Task<ResultService> TaoRoleMoi(CreateRole role);
        Task<ResultService> ChinhSuaRole(string idRole, string roleName);
        Task<ResultService> XoaRole(string idRole);
        public JwtSecurityToken getToken(List<Claim> authClaims);
    }
}
