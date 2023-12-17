using AutoMapper;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel.MangaView;
using TestWebApi_v1.Models.ViewModel.UserView;
using TestWebApi_v1.Repositories;

namespace TestWebApi_v1.Models.ViewModel
{
    public class Mapper : Profile
    {
        private readonly IServices _Service;
        public Mapper(IServices services)
        {
            _Service = services;
            CreateMap<User, UserViewModel>();
            CreateMap<User, UserInfo>();
            CreateMap<Role, RoleViewModel>();
            CreateMap<BoTruyen, EditManga>();
            CreateMap<Bookmark, BookmarkView>();
            CreateMap<BoTruyen, BotruyenProfile>();
            CreateMap<BotruyenProfile, botruyenView>().ForMember(dest => dest.MangaImage, opt => opt.MapFrom(src =>
                src.MangaImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", src.MangaId, src.MangaImage) : null));
            CreateMap<BotruyenProfile, botruyenViewforTopmanga>().ForMember(dest => dest.MangaImage, opt => opt.MapFrom(src =>
                src.MangaImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", src.MangaId, src.MangaImage) : null));
            CreateMap<BoTruyenTopView, TopManga>().ForMember(dest => dest.MangaImage, opt => opt.MapFrom(src =>
                src.MangaImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", src.MangaId, src.MangaImage) : null)); ;
            CreateMap<BotruyenProfile, MangaFollowing>().ForMember(dest => dest.MangaImage, opt => opt.MapFrom(src =>
                src.MangaImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", src.MangaId, src.MangaImage) : null)); ;
            CreateMap<BoTruyen, Searchmanga>();
            CreateMap<KenhChatUser, KenhChatUser2>();
            CreateMap<TheLoai, CategoryView>();
            CreateMap<ChuongTruyen, chapterView2>();
            CreateMap<ThongbaoUser, NotificationView>();
            CreateMap<BinhLuan, CommentViewModel>();

        }
    }
}
