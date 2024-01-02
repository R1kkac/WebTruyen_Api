using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Service;
using TestWebApi_v1.Repositories;
using TestWebApi_v1.Service.Phone;
using TestWebApi_v1.Models.Account;
using TestWebApi_v1.Models.Roles;
using Newtonsoft.Json;
using TestWebApi_v1.Models.ViewModel.UserView;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestWebApi_v1.Service.MailService.Service;
using TestWebApi_v1.Service.MailService.Models;
using TestWebApi_v1.Service.Respone;

namespace TestWebApi_v1.Controllers
{
    [Route("Authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Models.DbContext.Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepo _userModel;
        private readonly SmsService _smsService;
        private readonly IMapper _mapper;
        private readonly IServiceRepo _sv;

        public AuthenticationController(UserManager<User> userManager, RoleManager<Models.DbContext.Role> roleManager, IUserRepo userModel,
            IConfiguration configuration, IEmailService emailService, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor,
            SmsService smsService,IMapper mapper, IServiceRepo sv)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _userModel = userModel;
            _smsService = smsService;
            _mapper = mapper;
            _sv = sv;
        }
        //Lấy thông tin người dùng
        [HttpGet("Infouser/{idUser}")]
        public async Task<UserInfo?> getInfoUser(string idUser)
        {
            var result = await _userModel.layThongTinNguoiDung(idUser);
            if (result!= null)
            {
                result.Avatar = getCurrenthttpContext() + "/Avatar/" + result.Avatar;
                return result;
            }
            return null;
        }
        //lấy avatar User
        [HttpGet("Avatar/{image}")]
        public IActionResult Avatar(string image)
        {
            var result = _sv.LayAvatarUser(image);
            if (System.IO.File.Exists(result))
            {
                return PhysicalFile(result, "image/jpeg");
            }
            return StatusCode(StatusCodes.Status404NotFound,
                              new ResponeStatus { Status = "Failed", Message = $"Not Found" });
        }
        //Lấy danh sách user
        [HttpGet]
        [Route("ListUser")]
        public async Task<IEnumerable<ResponeUser>> getListUser()
        {
            var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
            string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var user =await _userManager.Users.ToListAsync();
            var UserView = _mapper.Map<List<ResponeUser>>(user);
            foreach (var item in UserView)
            {
                var url = $"Avatar/{item.Avatar}";
                item.Avatar = _sv.LayUrlAnh(requestUrl, routeController, url) ?? null;
            }
            if (UserView == null)
                return new List<ResponeUser>() { };
            return UserView;
        }
        //Xác thực email 
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            //tìm user
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                //xác thực email thông qua token (totable=> User=> colum=> EmailConfirm)
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                        new ResponeStatus { Status = "Success", Message = "Email Verified Successfully!" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                        new ResponeStatus { Status = "Error", Message = "Email Doesn't Exist!" });
        }
        //Trả về model với 2 tham số là token và email phục vụ cho hàm {ForgotPassword=> forgotPasswordLink}
        [HttpGet("Reset-password")]
        public IActionResult Resetpassword(string token, string email)
        {
            var model = new ResetPassword { token = token, email = email };
            return Ok(new
            {
                model
            });
        }
        //thêm user
        [HttpPost("Register")]
        //[Route("Register")]
        public async Task<IActionResult> Register([FromForm] string register)//, [FromForm] IFormFile? Avatar
        {
            var result = await _userModel.DangKytaiKhoan(register);//Avatar
            if (result.Value == true)
            {
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new {token= result.Message,email= result.email }, Request.Scheme);
                var message = new Message(new string[] { result.email! }, "Confirm Email", confirmationLink!);
                _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status201Created,
                new ResponeStatus { Status = "Success", Message = $"Tài khoản đã được tạo và email xác thực đã được gửi đến {result.email}!" });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                new ResponeStatus { Status = "Failed", Message = result.Message });
        }
        //đăng nhập
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginUser loginUser)
        {
            var user = await _userManager.FindByNameAsync(loginUser.UserName);
            if( user!= null)
            {
                user.Avatar = getCurrenthttpContext() + "/Avatar/" + user.Avatar;

            }
            //đăng nhập thông qua mật khẩu được gửi qua email
            /*LoginWith2FA*/
            //if (user.TwoFactorEnabled)
            //{
            //    await _signInManager.SignOutAsync();
            //    await _signInManager.PasswordSignInAsync(user, loginUser.Password, false, true);
            //    var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            //    var message = new Message(new string[] { user.Email! }, "Confirm Email", token);
            //    _emailService.SendEmail(message);

            //    return StatusCode(StatusCodes.Status200OK,
            //          new Respone { Status = "Success", Message = $"We have sent and OTP to your email {user.Email}" });
            //}
            if (user != null && await _userManager.CheckPasswordAsync(user, loginUser.Password))
            {
                //tạo claim cho tài khoản đăng nhập có thể thêm các tham số tùy chọn {new Claim("Avatar",user.Avatar)}
                var authClaim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(ClaimTypes.NameIdentifier,user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                };
                var userRole = await _userManager.GetRolesAsync(user);
                foreach (var role in userRole)
                {
                    authClaim.Add(new Claim(ClaimTypes.Role, role));
                }

                var jwtToken = _userModel.getToken(authClaim);

                ////tạo cookie, trả về respone thông qua header
                // string tokencookie = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                // _httpContextAccessor.HttpContext!.Response.Cookies.Append("Cookie-JWT", tokencookie,
                //    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(1) });
                //_httpContextAccessor.HttpContext.Session.SetString(user.Id.ToString(),user.Id.ToString());
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expiration = jwtToken.ValidTo,
                    userdata = _mapper.Map<ResponeUser>(user),
                });

            }
            return Unauthorized();
        }
        //đăng nhập thông qua mã 2FA được gửi qua email phần login /*LoginWith2FA*/
        [HttpPost]
        [Route("Login-2FA")]
        public async Task<IActionResult> Login2FA(string code, string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            var Signin = await _signInManager.TwoFactorSignInAsync("Email", code, false, false); //Email phải được confirm mới có thể sử dụng
            if (Signin.Succeeded)
            {
                if (user != null)
                {
                    var authClaim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                    var userRole = await _userManager.GetRolesAsync(user);
                    foreach (var role in userRole)
                    {
                        authClaim.Add(new Claim(ClaimTypes.Role, role));
                    }
                    var jwtToken = _userModel.getToken(authClaim);
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                        expiration = "Token will be expired in: " + jwtToken.ValidTo
                    });
                }

            }
            return StatusCode(StatusCodes.Status404NotFound,
                new ResponeStatus { Status = "Failed", Message = $"Invalid Code" });

        }
        [HttpPost]
        [AllowAnonymous]
        [Route("Forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromForm] string email)
        {
            var thisemail = JsonConvert.DeserializeObject<string>(email!);
            var clienturl = "http://localhost:4200/user/resetpassword"; ;
            var user = await _userManager.FindByEmailAsync(thisemail);
            if (user != null)
            {
                //tạo token reset password và gửi về email
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var uriBuilder = new UriBuilder(clienturl);
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                query["token"] = token;
                query["email"] = user.Email;
                uriBuilder.Query = query.ToString();
                string url = uriBuilder.ToString();
                var message = new Message(new string[] { user.Email! }, "ResetPassword", url!);
                _emailService.SendEmail(message);

                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = $"Yêu cầu thay đổi mật khẩu đã được gửi tới email {user.Email}. Vui lòng kiểm tra email của bạn!." });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                    new ResponeStatus { Status = "Errr", Message = $"Không thể gửi yêu cầu đến email của bạn, vui lòng thử lại" });
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("Reset-password")]
        public async Task<IActionResult> Resetpassword([FromForm] string email, [FromForm] string token, [FromForm] string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var resetResult = await _userManager.ResetPasswordAsync(user, token, password);
                if (!resetResult.Succeeded)
                {
                    foreach (var error in resetResult.Errors)

                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return Ok(ModelState);
                }
                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = $"Password Changed Request Is Sent On Email {user.Email}. Please Open Your Email & Changed New Password" });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                    new ResponeStatus { Status = "Errr", Message = $"Couldn't send to email. Please truy again!" });
        }
        //reset password
        //[HttpPost]
        //[AllowAnonymous]
        //[Route("Forgot-password")]
        //public async Task<IActionResult> ForgotPassword([Required] string email)
        //{
        //    var user = await _userManager.FindByEmailAsync(email);
        //    if (email != null)
        //    {
        //        //tạo token reset password và gửi về email
        //        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        var forgotPasswordLink = Url.Action(nameof(Resetpassword), "Authentication", new { token, email = user.Email }, Request.Scheme);
        //        var message = new Message(new string[] { user.Email! }, "Forgot Passwrd Link", forgotPasswordLink!);
        //        _emailService.SendEmail(message);

        //        return StatusCode(StatusCodes.Status200OK,
        //            new Respone { Status = "Success", Message = $"Password Changed Request Is Sent On Email {user.Email}. Please Open Your Email & Changed New Password" });
        //    }
        //    return StatusCode(StatusCodes.Status400BadRequest,
        //            new Respone { Status = "Errr", Message = $"Couldn't send to email. Please truy again!" });
        //}
        //Thay đổi mật khẩu với token được gừi đến email 
        //[HttpPost]
        //[AllowAnonymous]
        //[Route("Reset-password")]
        //public async Task<IActionResult> Resetpassword(ResetPassword reset)
        //{
        //    var user = await _userManager.FindByEmailAsync(reset.email);
        //    if (user != null)
        //    {
        //        var resetResult = await _userManager.ResetPasswordAsync(user, reset.token, reset.password);
        //        if (!resetResult.Succeeded)
        //        {
        //            foreach (var error in resetResult.Errors)
        //            {
        //                ModelState.AddModelError(error.Code, error.Description);
        //            }
        //            return Ok(ModelState);
        //        }
        //        return StatusCode(StatusCodes.Status200OK,
        //            new Respone { Status = "Success", Message = $"Password Changed Request Is Sent On Email {user.Email}. Please Open Your Email & Changed New Password" });
        //    }
        //    return StatusCode(StatusCodes.Status400BadRequest,
        //            new Respone { Status = "Errr", Message = $"Couldn't send to email. Please truy again!" });
        //}
        //Sửa đổi thông tin của tài khoản Name{ tên đại diện}, Avatar, Password, SDT
        [HttpPut]
        [Route("EditUser")]
        public async Task<IActionResult> EditUser([FromForm] string User, [FromForm] IFormFile? Avatar)
        {
            var result = await _userModel.SuaThongTinTaiKhoan(User, Avatar);
            if (result.Value == true)
            {
                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = result.Message });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                    new ResponeStatus { Status = "Failed", Message = result.Message });
        
        }
        [HttpPut]
        [Route("change_password_user")]
        public async Task<bool> changePasswordUser([FromForm] string User)
        {
            var result= await _userModel.thayDoiMatKhau(User);
            return result;
        }
        //Thay đối số điện thoại thông qua token được gửi đến email (có thể dùng sms phone number)
        [HttpPut("ChangePhone")]
        private async Task<IActionResult> changePhoneNumber(string email, string newNum, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var result = await _userManager.ChangePhoneNumberAsync(user, newNum, token);
            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK,
                   new ResponeStatus { Status = "Success", Message = "Change password Successfully!" });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
             new ResponeStatus { Status = "Failed", Message = "Change Phone Number failed!" });
        }
        //Xóa tải khoản 
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var result = await _userModel.XoataiKhoan(email);
            if (result.Value == true)
            {
                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = result.Message });
            }
            return StatusCode(StatusCodes.Status403Forbidden,
                    new ResponeStatus { Status = "Failed", Message = result.Message });
        }
        //Thêm Role cho user {User có thể có nhiều role vd: User,Mod,vv....}
        [HttpPost("AddRoleUser")]
        public async Task<IActionResult> AddRoleUser([FromForm] string idUser, [FromForm] string role)
        {
            var result = await _userModel.ThemRoleUser(idUser, role);
            if (result.Value == true)
            {
                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = result.Message });
            }
            return StatusCode(StatusCodes.Status403Forbidden,
                    new ResponeStatus { Status = "Failed", Message = result.Message });
        }
        //Xóa role User
        [HttpDelete("DeleteRole")]
        public async Task<IActionResult> deleteRoleUser(string idUser, string role)
        {
            var result = await _userModel.XoaRoleUser(idUser, role);
            if (result.Value == true)
            {
                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = result.Message });
            }
            return StatusCode(StatusCodes.Status403Forbidden,
                    new ResponeStatus { Status = "Failed", Message = result.Message });
        }
        //Lấy danh sách role
        [HttpGet]
        [Route("ListRole")]
        public async Task<IEnumerable<ResponeView>> getlistRole()
        {
            var result =await _roleManager.Roles.ToListAsync();
            var RoleView = _mapper.Map<List<ResponeView>>(result);
            if (result == null)
                return new List<ResponeView>() { };
            return RoleView;
        }
        //Tạo mới role
        [HttpPost]
        [Route("CreateNewRole")]
        public async Task<IActionResult> CreateRoles(Models.Roles.RoleRequest role)
        {
            var result = await _userModel.TaoRoleMoi(role);
            if (result.Value == true)
            {
                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = result.Message });
            }
            return StatusCode(StatusCodes.Status403Forbidden,
                    new ResponeStatus { Status = "Failed", Message = result.Message });
        }
        //Sửa Role
        [HttpPut]
        [Route("EditSimpleRole")]
        public async Task<IActionResult> EditRole(string idRole, string roleName)
        {
            var result = await _userModel.ChinhSuaRole(idRole, roleName);
            if (result.Value == true)
            {
                return StatusCode(StatusCodes.Status200OK, 
                    new ResponeStatus { Status = "Success", Message = result.Message });
            }
            return StatusCode(StatusCodes.Status403Forbidden,
                    new ResponeStatus { Status = "Failed", Message = result.Message });
        }
        //Xóa Role
        [HttpDelete]
        [Route("DeleteSimpleRole")]
        public async Task<IActionResult> DeleteRole(string idRole)
        {
            var result = await _userModel.XoaRole(idRole);
            if (result.Value == true)
            {
                return StatusCode(StatusCodes.Status200OK,
                    new ResponeStatus { Status = "Success", Message = result.Message });
            }
            return StatusCode(StatusCodes.Status403Forbidden,
                    new ResponeStatus { Status = "Failed", Message = result.Message });
        }
        private string getCurrenthttpContext()
        {
            var routeAttribute = ControllerContext.ActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(RouteAttribute), false).FirstOrDefault() as RouteAttribute;
            string routeController = (routeAttribute != null) ? routeAttribute.Template : "";
            string requestUrl = $"{Request.Scheme}://{Request.Host.Value}/";
            var result = $"{requestUrl}{routeController}";
            return result;
        }
    }
}
