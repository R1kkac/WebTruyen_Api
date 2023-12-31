﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Transactions;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.Account;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.Roles;
using TestWebApi_v1.Models.ViewModel.UserView;
using TestWebApi_v1.Service.MailService.Models;
using TestWebApi_v1.Service.MailService.Service;
using TestWebApi_v1.Service.Phone;
using TestWebApi_v1.Service.Respone;
using Twilio.Jwt.AccessToken;

namespace TestWebApi_v1.Repositories
{
    public class UserRepo:IUserRepo
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Models.DbContext.Role> _roleManager;
        private readonly SignInManager<User> _SignUser;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IMangaRepo _mangaModel;
        private readonly WebTruyenTranh_v2Context _db;
        private readonly IWebHostEnvironment _evn;
        private readonly IServiceRepo _sv;
        private readonly IMapper _mapper;

        public UserRepo(UserManager<User> userManager, RoleManager<Models.DbContext.Role> roleManager,IConfiguration configuration,IMapper mapper,
            IEmailService emailService, IMangaRepo mangaModel, WebTruyenTranh_v2Context db, IWebHostEnvironment evn, IServiceRepo sv,
            SignInManager<User> SignUser)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
            _mangaModel = mangaModel;
            _db = db;
            _evn = evn;
            _sv = sv;
            _mapper = mapper;
            _SignUser = SignUser;
        }
        //User
        //Đăng ký User
        public async Task<ResponeRegister> DangKytaiKhoan(string User)//, IFormFile? Avatar
        {
            var register = JsonConvert.DeserializeObject<RegisterUser>(User)!;
            var role = "User";
            //kiểm tra email có tồn tại trong CSDL hay không
            var userExist = await _userManager.FindByEmailAsync(register.Email);
            if (userExist != null)
            {
                return new ResponeRegister { Value = false, Message = "Tài khoản Đã tồn tại!",email=register.Email };
            }
            User user = new()
            {
                Name = register.UserName,
                Email = register.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = register.UserName,
                JoinDate = DateTime.UtcNow,
                //TwoFactorEnabled = true,
                //PhoneNumber = register.PhoneNumber,
                //Avatar=(Avatar!=null) ?Avatar.FileName:null
            };
            //await _smsService.SendAsync(register.PhoneNumber!); thiếu sdt from phải mua
            //Kiểm tra role có hợp lệ và tạo user
            if (await _roleManager.RoleExistsAsync(role))
            {
                var result = await _userManager.CreateAsync(user, register.Password);
                if (!result.Succeeded)
                {
                    return new ResponeRegister { Value = false, Message = $"Tạo tài khoản {user.Email} thất bại", email= user.Email };
                }
                //string mangaImagePath = Path.Combine(_evn.ContentRootPath, "Manga","Avatar");
                //if(Avatar!=null) await _sv.UpLoadimage(Avatar, mangaImagePath);
                //Thêm role cho user (toTable=>UserRoles)
                await _userManager.AddToRoleAsync(user, role);
                //tạo token xác thực và gửi đến email đăng ký (totable=> User=> colum=> emailConfirm)
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                //Trả về true và token confirm email nếu tạo được
                return new ResponeRegister { Value = true, Message = token ,email=user.Email};
            }else{
                return new ResponeRegister { Value = false, Message = $"Đã xảy ra lỗi trong quá trình tạo tài khoản {user.Email} ",email= user.Email };
            }
        }
        public async Task<bool> thayDoiMatKhau(string userchangepassword)
        {
            var user = JsonConvert.DeserializeObject<changePasswordUser>(userchangepassword)!;
            var TaiKhoan = await _userManager.FindByEmailAsync(user.email);
            if (TaiKhoan != null)
            {
                var checkPassword = _userManager.PasswordHasher.VerifyHashedPassword(TaiKhoan, TaiKhoan.PasswordHash, user.oldpassword);
                if(checkPassword == PasswordVerificationResult.Success)
                {
                    var result = await _userManager.ChangePasswordAsync(TaiKhoan, user.oldpassword, user.newpassword);
                    if (result.Succeeded)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public async Task<ResponeData> SuaThongTinTaiKhoan(string User,IFormFile? Avatar)
        {
            var user = JsonConvert.DeserializeObject<EditUser>(User)!;
            var TaiKhoan = await _userManager.FindByEmailAsync(user.email);
            if (TaiKhoan != null)
            {
                string mangaImagePath = Path.Combine(_evn.ContentRootPath, "Manga", "Avatar");
                var filepath = $"{mangaImagePath}/{TaiKhoan.Avatar}";
                _sv.DeleteImage(filepath);
                if (Avatar != null) await _sv.UpLoadimage(Avatar, mangaImagePath);
                TaiKhoan.Avatar = Avatar?.FileName ?? TaiKhoan.Avatar;
                TaiKhoan.Name = (user.name!.Any())? user.name: TaiKhoan.Name;
                TaiKhoan.PhoneNumber= (user.phone!.Any()) ? user.phone : TaiKhoan.PhoneNumber;
                ////Thay đổi mật khẩu với 2 tham số là mật khẩu cũ tự nhập chưa hash và mật khảu mới
                //await _userManager.ChangePasswordAsync(TaiKhoan, TaiKhoan.PasswordHash, user.NewPassword);

                //tạo token để gửi thay đổi số điện thoại
                var token = await _userManager.GenerateChangePhoneNumberTokenAsync(TaiKhoan, user.phone);
                var message = new Message(new string[] { user.email! }, "Confirm PhonNumber", token);
                _emailService.SendEmail(message);

                //cập nhật thông tin user hiện tại
                var resutl = await _userManager.UpdateAsync(TaiKhoan);
                if (resutl.Succeeded)
                {
                    return new ResponeData { Value=true, Message=$"Sửa thông tin user {TaiKhoan.Email} thành công"};
                }else{
                    return new ResponeData { Value = false, Message = "Sửa thông tin thất bại" };
                }
            }
            return new ResponeData { Value=false, Message="Đã xảy ra lỗi" };
        }
        public async Task<UserInfo?> layThongTinNguoiDung(string idUser)
        {
            var result = await _userManager.FindByIdAsync(idUser);
            var Roles = await _userManager.GetRolesAsync(result);
            var user = _mapper.Map<UserInfo>(result);
            user.Role = new List<string>(Roles);
            //user.Role = Roles;
            if (user != null)
            {
                return user;
            }
            return null;
        }
		//Lấy role User
		public async Task<IEnumerable<ResponeRole>> GetUserRolesAsync(string userId)
		{
			var userRoles = await _db.UserRoles
				.Where(ur => ur.UserId == userId)
				.Include(ur => ur.role)
				.Select(ur => new ResponeRole
				{
					Id = ur.RoleId,
					Name = ur.role.Name
				})
				.ToListAsync();

			return userRoles;
		}
		/// <summary>
		/// Lưu ý: muốn xóa tài khoản thì phải xóa botruyen trước nhưng bộ truyện còn có folder và image 
		/// nên không thể xóa theo cách thông thường
		/// </summary>
		/// <param name="email">Email user cần xóa</param>
		/// <returns>Trả về thành công hay thất bại</returns>
		public async Task<ResponeData> XoataiKhoan(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var dsTruyen = user.BoTruyens.ToList();
            foreach (var item in dsTruyen)
            {
                await _mangaModel.XoaTruyen(user.Id, item.MangaId);
            }
            //cần phải xóa tất cả bộ truyện do user tạo trước khi xóa user
            var result = await _userManager.DeleteAsync(user);
            string mangaImagePath = Path.Combine(_evn.ContentRootPath, "Manga", "Avatar");
            _sv.DeleteImage(mangaImagePath);
            if (result.Succeeded)
            {
                return new ResponeData { Value = true, Message = $"Xóa tài khoản {user.Email} thành công" }; ;
            }
            return new ResponeData { Value = false, Message = $"Xóa tài khoản {user.Email} thất bại" };
        }


        //User và Role
        //Thêm role mới vào user
        public async Task<ResponeData> ThemRoleUser(string idUser, string role)
        {
            var result = await _userManager.FindByIdAsync(idUser);
            if (result != null)
            {
                var b = await _userManager.AddToRoleAsync(result, role);
                if (b.Succeeded)
                {
                    return new ResponeData { Value = true, Message = $"Đã Thêm Role {role} vào User {result.Name} thành công" };
                }
                return new ResponeData { Value = false, Message = $"Thêm Role {role} vào User {result.Name} thất bại" };
            }
            return new ResponeData { Value = false, Message = $"Đã xảy ra lỗi" }; ;
        }
        //Xóa role cho User
        public async Task<ResponeData> XoaRoleUser(string idUser, string role)
        {
            var result = await _userManager.FindByIdAsync(idUser);
            if (result != null)
            {
                var b = await _userManager.RemoveFromRoleAsync(result, role);
                if (b.Succeeded)
                {
                    return new ResponeData { Value = true, Message = $"Đã xóa Role {role} cho User {result.Name}!" };
                }
                return new ResponeData { Value = false, Message = $"Xóa thất bại!" };
            }
            return new ResponeData { Value = false, Message = $"Đã xảy ra lỗi trong quá trình xóa!" };
        }


        //Role
        //Tạo role mới
        public async Task<ResponeData> TaoRoleMoi(Models.Roles.RoleRequest role)
        {
            var existRole =await _roleManager.FindByNameAsync(role.Name);
            if (existRole == null)
            {
                Models.DbContext.Role newRole = new()
                {
                    Name = role.Name,
                    NormalizedName = role.Name
                };
                var result = await _roleManager.CreateAsync(newRole);
                if (result.Succeeded)
                    return new ResponeData {Value=true, Message=$"Role {role.Name} đã được tạo thành công" };
            }
            return new ResponeData { Value = false, Message = $"Tạo thất bại" };
        }
        //Sửa role
        public async Task<ResponeData> ChinhSuaRole(string idRole, string roleName)
        {
            var a = await _roleManager.FindByIdAsync(idRole);
            if (a != null)
            {
                a.Name = roleName;
                a.NormalizedName = roleName.ToLower();
                var result = await _roleManager.UpdateAsync(a);
                if (result.Succeeded)
                    return new ResponeData { Value = true, Message = $"Đã sửa Role" };
            }
            return new ResponeData { Value = false, Message = $"Đã xảy ra lỗi" };
        }
        //Xóa role
        public async Task<ResponeData> XoaRole(string idRole)
        {
            var a = await _roleManager.FindByIdAsync(idRole);
            if (a != null)
            {
                var result = await _roleManager.DeleteAsync(a);
                if (result.Succeeded)
                    return new ResponeData { Value=true, Message="Đã xóa thành công"};
            }
            return new ResponeData { Value=false, Message="Xóa thất bại"};
        }
        //Khác
        //Tạo token JWT với tham số là danh sách claim được tạo ra 
        public JwtSecurityToken getToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires:DateTime.UtcNow.AddDays(7),
                claims:authClaims,
                signingCredentials:new SigningCredentials(authSigningKey,SecurityAlgorithms.HmacSha256)
                );
            return token;
        }


        public async Task UpLoadimageUser(IFormFile imageFile, string filePath)
        {
            using (var stream = new FileStream(filePath + "\\" + imageFile.FileName, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
                stream.Close();
            }
        }

      
        /*[HttpGet]
public IActionResult SendEmail()
{
  var message = new Message(
      new string[] { "haileds939@gmail.com" }, //Đối tượng gửi
      "Test", //Tiêu đề
      "Test sendEmail" //Nội dung
     );
  _emailService.SendEmail(message);
  return StatusCode(StatusCodes.Status200OK, new Respone
  {
      Status = "Success",
      Message = "Email send Successfully"
  });
}*/
    }
}
