using AutoMapper;
using TestWebApi_v1.Models.Account;
using TestWebApi_v1.Models;
using TestWebApi_v1.Repositories;
using TestWebApi_v1.Service.Hubs;
using TestWebApi_v1.Service.MailService.Service;
using TestWebApi_v1.Service.Phone;
using Microsoft.AspNetCore.SignalR;

namespace TestWebApi_v1.Service
{
    public static class DependencyInjection
    {
        public static void ConfigureDI(this IServiceCollection services)
        {
            //Khai báo các lớp và inteface đã tạo
            //Database
            services.AddScoped<WebTruyenTranh_v2Context>();
            //truyen tranh
            services.AddScoped<IMangaRepo, MangaRepo>();
            //truyen tanh va nguoi dung
            services.AddScoped<IUserRepo, UserRepo>();
            //Check token
            services.AddScoped<ICheckToken, CheckToken>();
            //UserManga
            services.AddScoped<IUserMangaRepo, UserMangaRepo>();
            //-- những dịch vụ service như sendmail hay mapper thì chỉ cần tạo một lần và dùng suốt vòng đời của ứng dụng nen dùng singleton
            //-- còn những dịch vụ yêu cầu http sẽ được khởi tạo độc lập với nhau
            //send email
            services.AddSingleton<IEmailService, EmailService>();
            //sms phone
            services.AddSingleton<SmsService>();
            //Services
            services.AddSingleton<IServiceRepo, ServiceRepo>();
            //SignalR
            services.AddSingleton<ChatRealTimeService>();

            //Thêm automapper
            //builder.Services.AddAutoMapper(typeof(Program));
            services.AddSingleton(provider => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new TestWebApi_v1.Models.ViewModel.Mapper(provider.GetService<IServiceRepo>()!));
            }).CreateMapper());
            //khai signalR để dùng trong controller
            services.AddScoped<RealTimeService>();
            //Khai CustomIUserIdProvider để thay đổi userid mặc định
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            //Add auto cache {mybackgroundService}
            services.AddHostedService<BackgroundService>();
            services.AddHostedService<MyBackgroundService2>();
        }
    }
}
