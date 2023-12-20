using AutoMapper;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using TestWebApi_v1.Models;
using TestWebApi_v1.Models.DbContext;
using TestWebApi_v1.Models.TruyenTranh.MangaView;
using TestWebApi_v1.Models.ViewModel;
using TestWebApi_v1.Models.ViewModel.MangaView;
using Twilio.Base;
using X.PagedList;
using static StackExchange.Redis.Role;

namespace TestWebApi_v1.Repositories
{
    public class MangaModel:IMangaModel
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IWebHostEnvironment _env;
        private WebTruyenTranh_v2Context _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IServices _sv;
        private static string data = "Manga";
        private readonly IDistributedCache _cache;
        public MangaModel(IWebHostEnvironment webHostEnvironment,WebTruyenTranh_v2Context db, UserManager<User> user,
            RoleManager<Role> role, IMapper mapper, IServices sv,IMemoryCache memoryCache,IDistributedCache cache)
        {
            _db = db;
            _env = webHostEnvironment;
            _userManager = user;
            _roleManager = role;
            _mapper = mapper;
            _sv = sv;
            _memoryCache= memoryCache;
            _cache = cache;
        }
        //------------------------------------------------------------------------------------------------------------//
        //-------------------------------------------------Manga-----------------------------------------------------//
        //Dữ liệu search
        public async Task<IEnumerable<Searchmanga>> search()
        {
            Stopwatch t = new Stopwatch();
            t.Start();
            var result = await _db.BoTruyens.ToListAsync();
            var data = _mapper.Map<List<Searchmanga>>(result);
            t.Stop();
            Console.WriteLine("Elasped Time:" + t.Elapsed);
            if (data != null)
                return data;
            return new List<Searchmanga> { };
        }
        //Search thời gian thực
        public async Task<IEnumerable<Searchmanga>> responeSearch(string value, string url)
        {
            var result=await _db.BoTruyens.Where(x=>x.MangaName.Contains(value)).ToListAsync();
            foreach(var x in result)
            {
                x.MangaImage = $"{url}/{x.MangaId}/{x.MangaImage}";
            }
            var data = _mapper.Map<List<Searchmanga>>(result);
            return data;
        }
        //Gọi danh sác truyện theo page với 3 tham số currentUrl, route, page
        public async Task<IEnumerable<botruyenView>> LayDanhSachTruyenTheoPage(int? page, string requestUrl,string routeController)
        {
            //bộ nhớ cục bộ
            if (_memoryCache.TryGetValue("ListManga", out List<botruyenView> list))
            {
                return list;
            }
            /*bộ nhớ phân táng IDistributedCache
            var aad = _cache.GetString("ListManga");*/
            int pageSize = 10; // Số lượng bản ghi trên mỗi trang
            int pageNumber = (page ?? 1); // Trang hiện tại
            var dsbotruyen =await _db.BoTruyens.AsNoTracking().OrderByDescending(x=>x.Dateupdate).ToPagedListAsync(pageNumber, pageSize);
            var result =new List<botruyenView>();
            foreach (var a in dsbotruyen)
            {
                RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                var map1 = _mapper.Map<BotruyenProfile>(a); 
                map1.requesturl = requestUrl;
                map1.routecontroller = routeController;
                var mapmanga = _mapper.Map<botruyenView>(map1);
                mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                List<ChuongTruyen> listChapter= await _db.ChuongTruyens.Where(x=> x.MangaId == a.MangaId).OrderByDescending(y=>y.ChapterDate)
                    .Take(3).ToListAsync();
                var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
                var mangaview= await _db.ViewCounts.Where(x=> x.Id == a.MangaId).Select(y=> y.Viewbyyear).FirstOrDefaultAsync();
                var mapchapter= _mapper.Map<List<chapterView2>>(listChapter);
                mapmanga.ListChaper = mapchapter;
                mapmanga.Listcategory = listCategory;
                mapmanga.View = mangaview.ToString();
                result.Add(mapmanga);
            }
            //test thêm cache
            //YourMethod(result);       
            return result;
        }
        //tạo cache
        //Test thôi
        public void YourMethod(List<botruyenView> data)
        {
            // Lưu trữ dữ liệu vào cache
            _memoryCache.Set("Datacache", data, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) // Thời gian sống của cache, ví dụ 15 phút.
            });
        }
        //Chi tiết một bộ truyện cụ thể
        public async Task<botruyenView?> LayThongTinTruyen(string MangaId, string requestUrl, string routeController)
        {
            var a =await _db.BoTruyens.FindAsync(MangaId);
            if (a != null)
            {
                RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                var map1 = _mapper.Map<BotruyenProfile>(a);
                map1.requesturl = requestUrl;
                map1.routecontroller = routeController;
                var mapmanga = _mapper.Map<botruyenView>(map1);
                mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).OrderByDescending(y => y.ChapterDate)
                    .ToListAsync();
                var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
                var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbyyear).FirstOrDefaultAsync();
                var mapchapter = _mapper.Map<List<chapterView2>>(listChapter);
                int sum = 0;
                sum += await _db.BinhLuans.Where(y => y.MangaId.Equals(mapmanga.MangaId)).CountAsync();
                mapmanga.Comment = sum;
                mapmanga.ListChaper = mapchapter;
                mapmanga.Listcategory = listCategory;
                mapmanga.View = mangaview.ToString();

                return mapmanga;
            }else{
                return null;
            }
        }
        //Lấy ảnh bìa của bộ truyện
        public string? LayAnhTruyen(string imageManga)
        {
            // Đường dẫn đến thư mục chứa hình ảnh "Manga"
            string imagePath = Path.Combine(_env.ContentRootPath, "Manga", "Content", imageManga);

            // Kiểm tra xem hình ảnh có tồn tại không
            if (System.IO.File.Exists(imagePath))
            {
                return imagePath;
            }
            return null;
        }
        //Tạo bộ truyện
        public async Task<bool> TaoTruyen(string idUser, string Manga, IFormFile? MangaImage)
        {
            var mangaid = Guid.NewGuid().ToString().Substring(0, 6);
            var user = await _userManager.FindByIdAsync(idUser);
            var MangaData = JsonConvert.DeserializeObject<botruyenView>(Manga);
            if (MangaData != null)
            {
                BoTruyen a = new BoTruyen
                {
                    MangaId = mangaid,
                    MangaName = MangaData.MangaName!,
                    MangaDetails = MangaData.MangaDetails,
                    MangaImage = (MangaImage != null) ? MangaImage.FileName : null,
                    MangaAlternateName = MangaData.MangaAlternateName,
                    MangaAuthor = MangaData.MangaAuthor!,
                    MangaArtist = MangaData.MangaArtist,
                    MangaGenre = MangaData.MangaGenre!,
                    Id=user.Id
                };
                string d = "Manga";
                string FolderPath = _sv.OnGetFolderPath(d);
                string mangaImagePath = Path.Combine(FolderPath, "Content");
                if (_sv.CreateFolder(mangaid, FolderPath) == true)
                {
                    if (MangaImage != null) await _sv.UpLoadimage(MangaImage, mangaImagePath);
                    user.BoTruyens.Add(a);
                    //_db.BoTruyens.Add(a);
                    await _db.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }
        //Sửa thông tin của bộ truyện
        public async Task<bool> SuaTruyen(string Id,string Manga, IFormFile? MangaImage)
        {
            using( var transaction= _db.Database.BeginTransaction())
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<EditManga>(Manga);
                    var truyen = _db.BoTruyens.Find(result!.MangaId);
                    if (truyen != null && truyen!.Id!.Equals(Id))
                    {
                        string d = "Manga";
                        string FolderPath = _sv.OnGetFolderPath(d);
                        string mangaImagePath = Path.Combine(FolderPath, "Content");

                        string? a = truyen.MangaImage;
                        truyen.MangaId = result.MangaId;
                        truyen.MangaName = result.MangaName ?? truyen.MangaName;
                        truyen.MangaDetails = result.MangaDetails ?? truyen.MangaDetails;
                        truyen.MangaImage = (MangaImage != null) ? MangaImage.FileName : truyen.MangaImage;
                        truyen.MangaAlternateName = result.MangaAlternateName ?? truyen.MangaAlternateName;
                        truyen.MangaAuthor = result.MangaAuthor ?? truyen.MangaAuthor;
                        truyen.MangaArtist = result.MangaArtist ?? truyen.MangaArtist;
                        truyen.MangaGenre = result.MangaGenre ?? truyen.MangaGenre;
                        await _db.SaveChangesAsync();
                        if (MangaImage != null)
                        {
                            if (a != null) _sv.DeleteImage(mangaImagePath + "\\" + a);
                            await _sv.UpLoadimage(MangaImage, mangaImagePath);
                        }
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            
        }
        //Xóa bộ truyện
        public async Task<bool> XoaTruyen(string iduUser, string MangaId)
        {
            var user = await _userManager.FindByIdAsync(iduUser);
            var truyen =await _db.BoTruyens.FindAsync(MangaId);
            if (truyen != null)
            {
                string mangaFile = Path.Combine(_sv.OnGetFolderPath(data), truyen.MangaId);
                if (mangaFile != null) _sv.DeleteFolder(mangaFile);

                var chuongTruyens = _db.ChuongTruyens.Where(x => x.MangaId == truyen.MangaId).ToList();
                foreach (var chuong in chuongTruyens)
                {
                    var image = _db.ChapterImages.Where(x => x.ChapterId == chuong.ChapterId);
                    _db.ChapterImages.RemoveRange(image);
                }
                _db.ChuongTruyens.RemoveRange(chuongTruyens);
                user.BoTruyens.Remove(truyen);
                _db.BoTruyens.Remove(truyen);
                string mangaImagePath = Path.Combine(_sv.OnGetFolderPath(data), "Content");
                _sv.DeleteImage(mangaImagePath + "\\" + truyen.MangaImage);
                await _db.SaveChangesAsync(); /*không thể dùng await _db.savechange() ở đây vì sẽ xung đột với{UserModel
                                          => await _userManager.DeleteAsync(user)}; section này chưa kết thúc section khác đã tạo*/
                return true;
            }
            else {
                return false; 
            }
        }
        //Lấy truyện mới cập nhật
        public async Task<List<botruyenView>> getMangaNewUpdate(string requestUrl, string routeController)
        {
            using( var _context= _db)
            {
                List<botruyenView> data = new List<botruyenView>();
                var result =await _context.BoTruyens.OrderByDescending(item=> item.Dateupdate).Take(6).ToListAsync();
                foreach (var item in result)
                {
                    RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", item.MangaId).FirstOrDefaultAsync();
                    var map1 = _mapper.Map<BotruyenProfile>(item);
                    map1.requesturl = requestUrl;
                    map1.routecontroller = routeController;
                    var map2 = _mapper.Map<botruyenView>(map1);
                    map2.Rating = rating?.Rating.ToString() ?? "N/A";

                    data.Add(map2);
                }
                return data;
            }
        }
        //Lấy số trang truyện
        public async Task<int> getPageNumber()
        {
            try
            {
                var result = await _db.BoTruyens.CountAsync();
                return (result/10) + (result % 10 >0 ? 1:0);
            }
            catch(SqlException ex)
            {
                throw ex;
            }
        }
        //lấy top manga
        public async Task<List<botruyenViewforTopmanga>> getTopmanga(int page, int number, int type, string requestUrl)
        {
            List<botruyenViewforTopmanga> data = new List<botruyenViewforTopmanga>();
            if(type == 0)
            {
                var result = await _db.BoTruyens
                    .OrderByDescending(x => x.BotruyenViewCounts.Sum(y => y.Viewbyyear))
                    .ToPagedListAsync(page, number); 
                foreach (var a  in result)
                {
                    RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                    var map1 = _mapper.Map<BotruyenProfile>(a);
                    map1.requesturl = requestUrl;
                    map1.routecontroller = "Truyen-tranh";
                    var mapmanga = _mapper.Map<botruyenViewforTopmanga>(map1);
                    mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                    var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbyyear).FirstOrDefaultAsync();
                    var chaptercount =await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).CountAsync();
                    mapmanga.View = mangaview.ToString();
                    mapmanga.chaptercount = chaptercount.ToString();
                    mapmanga.mangaCount = result.Count().ToString();
                    var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
                    mapmanga.Listcategory = listCategory;
                    data.Add(mapmanga);
                }
                return data;
            }
            else
            {
                var result = await _db.TypeMangas
                    .Where(x => x.Id == type)
                    .SelectMany(y => y.Mangas)
                    .OrderByDescending(z => z.BotruyenViewCounts.Sum(d => d.Viewbyyear))
                    .ToPagedListAsync(page, number);
                foreach (var a in result)
                {
                    RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                    var map1 = _mapper.Map<BotruyenProfile>(a);
                    map1.requesturl = requestUrl;
                    map1.routecontroller = "Truyen-tranh";
                    var mapmanga = _mapper.Map<botruyenViewforTopmanga>(map1);
                    mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                    var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbyyear).FirstOrDefaultAsync();
                    var chaptercount = await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).CountAsync();
                    mapmanga.View = mangaview.ToString();
                    mapmanga.chaptercount = chaptercount.ToString();
                    mapmanga.mangaCount = result.Count().ToString();
                    data.Add(mapmanga);
                }
                return data;
            }

        }

        public async Task<List<botruyenViewforTopmanga>> danhSahcBotruyen(int type, int pagesize, int pagenumber, string requesurl)
        {
            List<botruyenViewforTopmanga> data = new List<botruyenViewforTopmanga>();
            IEnumerable<BoTruyen> botruyen = new List<BoTruyen>();
            switch (type)
            {
                case 0:
                    botruyen = await MangaByName(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 1:
                    botruyen = await MangaByNameDes(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 2:
                    botruyen = await MangaByStatus(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 3:
                    botruyen = await MangaByDateUpdate(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 4:
                    botruyen = await MangaByNumberOfChapter(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 5:
                    botruyen = await MangaByView(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                case 6:
                    botruyen = await MangaByRating(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
                default:
                    botruyen = await MangaByName(pagesize, pagenumber);
                    data = await mapMangaToMangaView(botruyen, requesurl);
                    break;
            }
            return data;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByName(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.OrderBy(x => x.MangaName).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByNameDes(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.OrderByDescending(x => x.MangaName).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByStatus(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.OrderByDescending(x => x.Status).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByDateUpdate(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.OrderByDescending(x => x.ChuongTruyens.Max(x=> x.ChapterDate)).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByNumberOfChapter(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.OrderByDescending(x => x.ChuongTruyens.Count).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByView(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.OrderByDescending(x => x.BotruyenViewCounts.Max(x=> x.Viewbyyear)).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<IEnumerable<BoTruyen>> MangaByRating(int pagesize, int pagenumber)
        {
            var result = await _db.BoTruyens.OrderByDescending(x => x.RatingMangas.Max(y=> y.Rating)).ToPagedListAsync(pagenumber, pagesize);
            return result;
        }
        public async Task<List<botruyenViewforTopmanga>> mapMangaToMangaView(IEnumerable<BoTruyen> listdata, string requesturl)
        {
            List<botruyenViewforTopmanga> data = new List<botruyenViewforTopmanga>();
            foreach (var a in listdata)
            {
                RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", a.MangaId).FirstOrDefaultAsync();
                var map1 = _mapper.Map<BotruyenProfile>(a);
                map1.requesturl = requesturl;
                map1.routecontroller = "Truyen-tranh";
                var mapmanga = _mapper.Map<botruyenViewforTopmanga>(map1);
                mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                var mangaview = await _db.ViewCounts.Where(x => x.Id == a.MangaId).Select(y => y.Viewbyyear).FirstOrDefaultAsync();
                var chaptercount = await _db.ChuongTruyens.Where(x => x.MangaId == a.MangaId).CountAsync();
                var listCategory = await _db.BoTruyens.Where(x => x.MangaId == a.MangaId).SelectMany(y => y.Genres).ToListAsync();
                mapmanga.Listcategory= listCategory;
                mapmanga.View = mangaview.ToString();
                mapmanga.chaptercount = chaptercount.ToString();
                mapmanga.mangaCount = listdata.Count().ToString();
                data.Add(mapmanga);
            }
            return data;
        }
        public async Task<int> numbermanga()
        {
            return await _db.BoTruyens.CountAsync();
        }

        //-----------------------------------------------End-Manga----------------------------------------------------//
        //-------------------------------------------------Chapter-----------------------------------------------------//

        //Lấy danh sách tất cả chương truyện
        public async Task<IEnumerable<ChuongTruyen>> LayTatcaChuongTruyen()
        {
            var dschapter =await _db.ChuongTruyens.ToListAsync();
            return dschapter;
        }
        //Tạo chương truyện của bộ truyện
        public async Task<bool> TaoChuongTruyen(string Chapter, List<IFormFile> MangaImage, List<string> MangaUrl)
        {
            var chuongTruyen = JsonConvert.DeserializeObject<ChuongTruyen>(Chapter);
            string FolderPath = _sv.OnGetFolderPath(data);
            string mangaPath = Path.Combine(FolderPath, chuongTruyen!.MangaId);
            if (_sv.CreateFolder(chuongTruyen.ChapterId, mangaPath) == true)
            {
                _db.ChuongTruyens.Add(chuongTruyen);
                if (MangaImage.Any() && MangaUrl.Any())
                {
                    foreach (var image in MangaImage)
                    {
                        string url = "";
                        int i = 0;
                        while (i < MangaUrl.Count)
                        {
                            url = MangaUrl[i];
                            i++;
                        }
                        await _sv.UpLoadimage(image, mangaPath + "\\" + chuongTruyen.ChapterId);
                        ChapterImage d = new ChapterImage
                        {
                            ImageName = image.FileName,
                            ImageUl = "https" + url,
                            ChapterId = chuongTruyen.ChapterId
                        };
                        chuongTruyen.ChapterImages.Add(d);
                    }
                }
                else if (MangaImage.Any() && MangaUrl.Any() == false)
                {
                    foreach (var image in MangaImage)
                    {
                        await _sv.UpLoadimage(image, mangaPath + "\\" + chuongTruyen.ChapterId);
                        ChapterImage d = new ChapterImage
                        {
                            ImageName = image.FileName,
                            ImageUl = null,
                            ChapterId = chuongTruyen.ChapterId
                        };
                        chuongTruyen.ChapterImages.Add(d);
                    }
                }
                else if (MangaImage.Any() == false && MangaUrl.Any() == true)
                {
                    var a = "https";
                    foreach (var url in MangaUrl)
                    {
                        ChapterImage d = new ChapterImage
                        {
                            ImageName = a + url,
                            ImageUl = a + url,
                            ChapterId = chuongTruyen.ChapterId
                        };
                        chuongTruyen.ChapterImages.Add(d);
                    }
                }
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        //Sửa chương truyện của bộ truyện
        public async Task<bool> SuaChuongTruyen(string Chapter, List<IFormFile> MangaImage, List<string> MangaUrl)
        {
            var chuongTruyen = JsonConvert.DeserializeObject<ChuongTruyen>(Chapter);
            string FolderPath = _sv.OnGetFolderPath(data);
            string mangaPath = Path.Combine(FolderPath, chuongTruyen!.MangaId, chuongTruyen.ChapterId);
            ChuongTruyen? x =await _db.ChuongTruyens.FindAsync(chuongTruyen.ChapterId);
            if (x != null)
            {
                x.ChapterId = (chuongTruyen.ChapterId ?? x.ChapterId);
                x.ChapterName = (chuongTruyen.ChapterName ?? x.ChapterId);
                x.ChapterTitle = (chuongTruyen.ChapterTitle ?? x.ChapterId);
                x.ChapterDate = x.ChapterDate;
                x.MangaId = chuongTruyen.MangaId;
                if (MangaImage.Any() && MangaUrl.Any())
                {
                    _sv.DeleteAllFilesInFolder(mangaPath);
                    var d = _db.ChapterImages.Where(x => x.ChapterId == chuongTruyen.ChapterId).ToList();
                    _db.ChapterImages.RemoveRange(d);
                    foreach (var image in MangaImage)
                    {
                        string url = "";
                        int i = 0;
                        while (i < MangaUrl.Count)
                        {
                            url = MangaUrl[i];
                            i++;
                        }
                        await _sv.UpLoadimage(image, mangaPath);
                        ChapterImage h = new ChapterImage
                        {
                            ImageName = image.FileName,
                            ImageUl = "https" + url,
                            ChapterId = x.ChapterId
                        };
                        x.ChapterImages.Add(h);
                    }
                }
                else if (MangaImage.Any() && MangaUrl.Any() == false)
                {
                    _sv.DeleteAllFilesInFolder(mangaPath);
                    var d = _db.ChapterImages.Where(x => x.ChapterId == chuongTruyen.ChapterId).ToList();
                    _db.ChapterImages.RemoveRange(d);
                    foreach (var image in MangaImage)
                    {
                        await _sv.UpLoadimage(image, mangaPath);
                        ChapterImage h = new ChapterImage
                        {
                            ImageName = image.FileName,
                            ImageUl = null,
                            ChapterId = x.ChapterId
                        };
                        x.ChapterImages.Add(h);
                    }
                }
                else if (MangaImage.Any() == false && MangaUrl.Any() == true)
                {
                    var a = "https";
                    var d = _db.ChapterImages.Where(x => x.ChapterId == chuongTruyen.ChapterId).ToList();
                    _db.ChapterImages.RemoveRange(d);
                    foreach (var url in MangaUrl)
                    {
                        ChapterImage h = new ChapterImage
                        {
                            ImageName = a + url,
                            ImageUl = a + url,
                            ChapterId = x.ChapterId
                        };
                        x.ChapterImages.Add(h);
                    }
                }
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        //Xóa chương truyện
        public async Task<bool> XoaChuongTruyen(string MangaId, string ChapterId)
        {
            var chuongtruyen = _db.ChuongTruyens.Find(ChapterId);
            if (chuongtruyen != null)
            {
                string mangaFile = Path.Combine(_sv.OnGetFolderPath(data), MangaId, ChapterId);
                if (mangaFile != null) _sv.DeleteFolder(mangaFile);
                _db.ChapterImages.RemoveRange(_db.ChapterImages.Where(x => x.ChapterId == chuongtruyen.ChapterId));
                _db.ChuongTruyens.Remove(chuongtruyen);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        //Lấy danh sách chương truyện của một bộ truyện cụ thể
        public async Task<IEnumerable<chapterView2>> DanhSachChuongCuaBoTruyen(string idManga, string requestUrl, string routeController)
        {
            var dschuong =await _db.ChuongTruyens.Where(x => x.MangaId == idManga).ToListAsync();
            var a = new List<chapterView2>();
            foreach (var item in dschuong)
            { 
                var chap= _mapper.Map<chapterView2>(item);
                a.Add(chap);
            }
            return a;
        }
        //public async Task<IEnumerable<chapterView>> DanhSachChuongCuaBoTruyen(string idManga, string requestUrl, string routeController)
        //{
        //    var dschuong = _db.ChuongTruyens.Where(x => x.MangaId == idManga).ToList();
        //    var a = new List<chapterView>();
        //    foreach (var item in dschuong)
        //    {
        //        var imageName = _db.ChapterImages.Where(x => x.ChapterId == item.ChapterId).Select(x => x.ImageName).ToList();
        //        for (var i = 0; i < imageName.Count; i++)
        //        {
        //            string b = _sv.getUrlImageforChapter(requestUrl, routeController, idManga, item.ChapterId, imageName[i]).Replace("http:", "http:");
        //            imageName[i] = (await _sv.checkUrlImage(b)) ? b : imageName[i];
        //        }
        //        chapterView aItem = new chapterView
        //        {
        //            Chapter_Id = item.ChapterId,
        //            Chapter_Name = item.ChapterName,
        //            Chapter_Title = item.ChapterTitle,
        //            Chapter_Date = item.ChapterDate.ToShortDateString(),
        //            Manga_Id = item.MangaId,
        //            Imagechapter = imageName.ToList()
        //        };
        //        a.Add(aItem);
        //    }
        //    return a;
        //}
        //Lấy danh sách ảnh của chương truyện
        public async Task<IEnumerable<string>> DanhSachAnhTheoChuong(string idManga, string idChapter, string requestUrl, string routeController)
        {
            string[] imageName = _db.ChapterImages.Where(x => x.ChapterId == idChapter).Select(x => x.ImageName).ToArray();
            for (int i = 0; i < imageName.Length; i++)
            {
                string a = _sv.getUrlImageforChapter(requestUrl, routeController, idManga, idChapter, imageName[i]).Replace("http:", "https:");
                imageName[i] = (await _sv.checkUrlImage(a)) ? a : imageName[i];
            }
            return imageName;
        }
        //Lấy ảnh của chương truyện
        public string LayAnh(string idManga, string idChapter, string image)
        {
            string imagePath = Path.Combine(_env.ContentRootPath, "Manga", idManga, idChapter, image);
            return imagePath;
        }

        //------------------------------------------------End-Chapter----------------------------------------------------//
        //-------------------------------------------------Categories-----------------------------------------------------//
        //Lấy danh sách thể loại
        public async Task<List<CategoryView>> getListCategory()
        {
            using(var _context= _db)
            {
                var data =await _context.TheLoais.ToListAsync();
                var map = _mapper.Map<List<CategoryView>>(data);  
                return map;
            }
        }
        //Lấy danh sách truyện theo thể loại truyện
        public async Task<List<botruyenView>> getMangaByCategory(string id, string requestUrl, string routeController)
        {
            using (var _context = _db)
            {
                int idx = int.Parse(id);
                List<botruyenView> data = new List<botruyenView>();
                var mangasInGenre = await _context.TheLoais
                .Where(tl => tl.GenreId == idx) // Thay yourGenreId bằng ID của thể loại bạn quan tâm
                .SelectMany(tl => tl.Mangas)
                .ToListAsync();
                foreach (var item in mangasInGenre)
                {
                    RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", item.MangaId).FirstOrDefaultAsync();
                    var map1 = _mapper.Map<BotruyenProfile>(item);
                    map1.requesturl = requestUrl;
                    map1.routecontroller = routeController;
                    var mapmanga = _mapper.Map<botruyenView>(map1);
                    mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                    List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == item.MangaId).OrderByDescending(y => y.ChapterDate)
                        .Take(3).ToListAsync();
                    var listCategory = await _db.BoTruyens.Where(x => x.MangaId == item.MangaId).SelectMany(y => y.Genres).ToListAsync();
                    var mapchapter = _mapper.Map<List<chapterView2>>(listChapter);
                    mapmanga.ListChaper = mapchapter;
                    mapmanga.Listcategory = listCategory;
                    data.Add(mapmanga);
                }

                return data;
            }
        }
        //Tìm kiếm truyện nâng cao theo thể loại
        public async Task<List<botruyenView>> getMangaByCategories(List<string> listCategories, string requestUrl, string routeController)
        {
            //var result = await _db.BoTruyens
            //            .Where(botruyen => listCategories.All(x => botruyen.Genres.Any(y => y.GenreId.ToString() == x)))
            //            .ToListAsync();

            var result = await (from botruyen in _db.BoTruyens
                                where botruyen.Genres.Any(x => listCategories.Contains(x.GenreId.ToString()))
                                select botruyen).ToListAsync();
            List<botruyenView> a = new List<botruyenView>();
            foreach(var item in result)
           {
                RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", item.MangaId).FirstOrDefaultAsync();
                var map1 = _mapper.Map<BotruyenProfile>(item);
                map1.requesturl = requestUrl;
                map1.routecontroller = routeController;
                var mapmanga = _mapper.Map<botruyenView>(map1);
                mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == item.MangaId).OrderByDescending(y => y.ChapterDate)
                    .Take(3).ToListAsync();
                var listCategory = await _db.BoTruyens.Where(x => x.MangaId == item.MangaId).SelectMany(y => y.Genres).ToListAsync();
                var mapchapter = _mapper.Map<List<chapterView2>>(listChapter);
                mapmanga.ListChaper = mapchapter;
                mapmanga.Listcategory = listCategory;
                a.Add(mapmanga);
            }
            return a;

        }
        //Tìm kiếm truyện nâng cao theo tất cả thể loại
        public async Task<string> getMangaByCategoriesAll(List<string> listCategories, string requestUrl)
        {
            List<BoTruyen> listmanga = await _db.BoTruyens
                .Include(boTruyen => boTruyen.Genres)
                .Include(chapter => chapter.ChuongTruyens.Take(3))
                .ToListAsync();
            var result = listmanga.Where(x => listCategories.All(y => x.Genres.Any(z => z.GenreId == int.Parse(y)))).ToList();
            List<botruyenView> a = new List<botruyenView>();
            foreach (var item in result)
            {
                RatingManga? rating = await _db.RatingMangas.FromSqlRaw("select * from RatingManga where Mangaid = @p0", item.MangaId).FirstOrDefaultAsync();
                var map1 = _mapper.Map<BotruyenProfile>(item);
                map1.requesturl = requestUrl;
                map1.routecontroller = "Truyen-tranh";
                var mapmanga = _mapper.Map<botruyenView>(map1);
                mapmanga.Rating = rating?.Rating.ToString() ?? "N/A";
                //List<ChuongTruyen> listChapter = await _db.ChuongTruyens.Where(x => x.MangaId == item.MangaId).OrderByDescending(y => y.ChapterDate)
                //    .Take(3).ToListAsync();
                //var listCategory = await _db.BoTruyens.Where(x => x.MangaId == item.MangaId).SelectMany(y => y.Genres).ToListAsync();
                var mapchapter = _mapper.Map<List<chapterView2>>(item.ChuongTruyens.ToList());
                mapmanga.ListChaper = mapchapter;
                mapmanga.Listcategory = item.Genres.ToList();
                a.Add(mapmanga);
            }
            var data = System.Text.Json.JsonSerializer.Serialize(a, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            });
            return data;

        }
        //Tải danh sách topmanga default
        public async Task<List<TopManga>> getTopMangaDefault(string requestUrl, string routeController)
        {
            var date = Topmangabydate(null, requestUrl, routeController);
            var month = Topmangabymonth(null, requestUrl, routeController);
            var year = TopmangabyYear(null, requestUrl, routeController);
            await Task.WhenAll(date, month, year);
            List<TopManga> data = new List<TopManga>();

            data.AddRange(date.Result);
            data.AddRange(month.Result);
            data.AddRange(year.Result);
            return data;
        }
        //lấy danh sách top manga theo ngày
        public async Task<List<TopManga>> Topmangabydate(int? page, string requestUrl, string routeController)
        {
            int pageNumber = (page ?? 1);
            int pagesize = 10;
            var result =await (from botruyen in _db.BoTruyens
                          join view in _db.ViewCounts
                          on botruyen.MangaId equals view.Id
                          select new BoTruyenTopView
                          {
                              MangaId = botruyen.MangaId,
                              MangaName = botruyen.MangaName,
                              MangaImage = botruyen.MangaImage,
                              View = view.Viewbydate,
                              requesturl= requestUrl,
                              routecontroller = routeController,
                          }).OrderByDescending(x => x.View).ToPagedListAsync(pageNumber, pagesize);
            var map= _mapper.Map<List<TopManga>>(result);
            map.ForEach(item => item.Typetop = "1");
            return map;
        }
        //lấy danh sách top manga theo tháng
        public async Task<List<TopManga>> Topmangabymonth(int? page, string requestUrl, string routeController)
        {
            int pageNumber = (page ?? 1);
            int pagesize = 10;
            var result = await (from botruyen in _db.BoTruyens
                                join view in _db.ViewCounts
                                on botruyen.MangaId equals view.Id
                                select new BoTruyenTopView
                                {
                                    MangaId = botruyen.MangaId,
                                    MangaName = botruyen.MangaName,
                                    MangaImage = botruyen.MangaImage,
                                    View = view.Viewbymonth,
                                    requesturl = requestUrl,
                                    routecontroller = routeController,
                                }).OrderByDescending(x => x.View).ToPagedListAsync(pageNumber, pagesize);
            var map = _mapper.Map<List<TopManga>>(result);
            map.ForEach(item => item.Typetop = "2");
            return map;
        }
        //lấy danh sách top manga theo năm
        public async Task<List<TopManga>> TopmangabyYear(int? page, string requestUrl, string routeController)
        {
            int pageNumber = (page ?? 1);
            int pagesize = 10;
            var result = await (from botruyen in _db.BoTruyens
                                join view in _db.ViewCounts
                                on botruyen.MangaId equals view.Id
                                select new BoTruyenTopView
                                {
                                    MangaId = botruyen.MangaId,
                                    MangaName = botruyen.MangaName,
                                    MangaImage = botruyen.MangaImage,
                                    View = view.Viewbyyear,
                                    requesturl = requestUrl,
                                    routecontroller = routeController,
                                }).OrderByDescending(x => x.View).ToPagedListAsync(pageNumber, pagesize);
            var map = _mapper.Map<List<TopManga>>(result);
            map.ForEach(item => item.Typetop = "3");
            return map;
        }
        //Tìm kiếm manga theo danh sách thể loại
        //public async Task<botruyenView> FindByListCategory(List<string> categories)
        //{
        //    using( var _context= _db)
        //    {
        //        var result=await _context.BoTruyens.Where(x=> categories.All( y=> x.MangaGenre.Contains(y))).ToListAsync();
        //        foreach(var item in result)
        //        {
        //            botruyenView
        //        }
        //    }
        //}
    }
}
