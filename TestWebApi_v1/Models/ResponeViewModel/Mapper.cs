using AutoMapper;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.ResponeViewModel.ResponeManga;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel.MangaView;
using TestWebApi_v1.Models.ViewModel.UserView;
using TestWebApi_v1.Repositories;

namespace TestWebApi_v1.Models.ViewModel
{
    public class Mapper : Profile
    {
        private readonly IServiceRepo _Service;
        public Mapper(IServiceRepo services)
        {
            _Service = services;           
            CreateMap<User, ResponeUser>();
            CreateMap<User, UserInfo>();
            CreateMap<Role, ResponeView>();
            CreateMap<BoTruyen, ResponeEditManga>();
            CreateMap<Bookmark, ResponeBookmark>();
            CreateMap<BoTruyen, ResponeMangaInfo>();
            CreateMap<ResponeMangaInfo, ResponeManga>().ForMember(dest => dest.MangaImage, opt => opt.MapFrom(src =>
                src.MangaImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", src.MangaId, src.MangaImage) : null));
            CreateMap<ResponeMangaInfo, botruyenViewforTopmanga>()
                .ForMember(dest => dest.MangaImage, opt => opt.MapFrom(src =>
                src.MangaImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", src.MangaId, src.MangaImage) : null));
            CreateMap<BoTruyenTopView, TopManga>().ForMember(dest => dest.MangaImage, opt => opt.MapFrom(src =>
                src.MangaImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", src.MangaId, src.MangaImage) : null)); ;
            CreateMap<ResponeMangaInfo, MangaFollowing>().ForMember(dest => dest.MangaImage, opt => opt.MapFrom(src =>
                src.MangaImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", src.MangaId, src.MangaImage) : null)); ;
            CreateMap<BoTruyen, Searchmanga>();
            CreateMap<KenhChatUser, KenhChatUser2>();
            CreateMap<TheLoai, ResponeCategory>();
            CreateMap<ChuongTruyen, chapterView2>();
            CreateMap<ThongbaoUser, ResponeNotification>();
            CreateMap<BinhLuan, ResponeComment>();

			CreateMap<MangaArtist, ResponeArtistInfo>();
			CreateMap<MangaArtist, ResponeArtist>();
			CreateMap<ResponeArtistInfo, ResponeArtist>().ForMember(dest => dest.ArtistImage, opt => opt.MapFrom(src =>
			   src.ArtistImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", "ArtistImage", src.ArtistImage) : null));

			CreateMap<MangaAuthor, ResponeAuthorInfo>();
			CreateMap<MangaAuthor, ResponeAuthor>();
			CreateMap<ResponeAuthorInfo, ResponeAuthor>().ForMember(dest => dest.AuthorImage, opt => opt.MapFrom(src =>
			   src.AuthorImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", "AuthorImage", src.AuthorImage) : null));


			CreateMap<ResponeMangaInfo, CRUDView>().ForMember(dest => dest.MangaImage, opt => opt.MapFrom(src =>
			   src.MangaImage != null ? _Service.getImageManga(src.requesturl ?? "", src.routecontroller ?? "", src.MangaId, src.MangaImage) : null));
		}
    }
}
