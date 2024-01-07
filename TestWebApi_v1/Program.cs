using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Service.Hubs;
using TestWebApi_v1.Service.MailService.Models;
using TestWebApi_v1.Service;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option => {
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth Api", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference=new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });

});

//Dbcontext
builder.Services.AddDbContext<WebTruyenTranh_v2Context>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Memory cache
builder.Services.AddMemoryCache();
//Phần Identity
builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<WebTruyenTranh_v2Context>()
    .AddDefaultTokenProviders();
//Thêm phương thức kiểm tra email
builder.Services.Configure<IdentityOptions>(options =>
    options.SignIn.RequireConfirmedEmail = true);
//Cấu hình password identity
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true; //có ký tự số từ 0-9
    options.Password.RequireLowercase = true; //ký tự thường
    options.Password.RequireNonAlphanumeric = true; //Ký tự đặc biệt
    options.Password.RequireUppercase = true; //Ký tự hoa
    options.Password.RequiredLength = 6; //độ dài tối thiểu password
    options.Password.RequiredUniqueChars = 1; 
});

//Authentication với JWT 
builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.SaveToken = true;
    option.RequireHttpsMetadata = false;
    //JwtBearer
    option.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        RequireExpirationTime = true,
        ValidIssuer = configuration["JWT:ValidIssuer"],
        ValidAudience = configuration["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
    };
    //signalr {kiểm tra người dùng có authorie hay chưa thông qua access_token gửi kèm từ client}
    option.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/Notification"))) //dùng đường dẫn tương đối, nếu dầy đủ thì dùng {url.StartsWith("https://localhost:7132/Notification") và   var path = context.HttpContext.Request.Path; } thay vì StartsWithSegments
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

//Cầu hình để có thể sử dụng nhiều cookie
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();

//Thêm mới cookie
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromSeconds(30);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});
//Cấu hình cookie
builder.Services.ConfigureApplicationCookie(option =>
{
    option.Cookie.Name = "Yahallo_Authentication";
    option.ExpireTimeSpan = TimeSpan.FromDays(7);
});
//Thêm cấu hình send email
var emailConfig = configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfigration>();

builder.Services.AddSingleton(emailConfig);
//Cấu hình access địa chỉ truy cập
builder.Services.AddCors(options =>
{
    options.AddPolicy("Policy",
         policy =>
         {
             policy.WithOrigins("http://localhost:4200/", "http://nafamiss.online/", "https://sonic-column-401016.web.app/", "https://sonic-column-401016.firebaseapp.com/")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetIsOriginAllowed((host)=> true);
         });
});
//Add SignalR
builder.Services.AddSignalR();
//DependencyInjection
builder.Services.ConfigureDI();
//bộ nhớ đệm phân táng
builder.Services.AddDistributedMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();

app.UseCors("Policy");
app.UseAuthentication();

app.UseAuthorization();
//dùng session
app.UseSession();
app.MapControllers();
app.MapHub<ChatRealTimeService>("/Notification");
app.Run();
