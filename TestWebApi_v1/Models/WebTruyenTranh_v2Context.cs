﻿using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestWebApi_v1.Models.DbContext;

namespace TestWebApi_v1.Models
{
    public partial class WebTruyenTranh_v2Context : IdentityDbContext<User,Role,string,IdentityUserClaim<string>,
        UserRoles,IdentityUserLogin<string>,IdentityRoleClaim<string>,IdentityUserToken<string>>
    {
        public WebTruyenTranh_v2Context()
        {
        }

        public WebTruyenTranh_v2Context(DbContextOptions<WebTruyenTranh_v2Context> options)
            : base(options)
        {
        }

        public virtual DbSet<BoTruyen> BoTruyens { get; set; } = null!;
        public virtual DbSet<ChapterImage> ChapterImages { get; set; } = null!;
        public virtual DbSet<ChuongTruyen> ChuongTruyens { get; set; } = null!;
        public virtual DbSet<TheLoai> TheLoais { get; set; } = null!;
        public virtual DbSet<BotruyenViewCount> ViewCounts { get; set; } = null!;
        public virtual DbSet<ThongbaoUser> Notification { get; set; } = null!;
        public virtual DbSet<Bookmark> bookmark { get; set; } = null!;
        public virtual DbSet<BinhLuan> BinhLuans { get; set; } = null!;
        public virtual DbSet<ReplyComment> ReplyComments { get; set; } = null!;
        public virtual DbSet<RatingManga> RatingMangas { get; set; } = null!;
        public virtual DbSet<TypeManga> TypeMangas { get; set; } = null!;
        public virtual DbSet<Datachat> Datachats { get; set; } = null!;
        public virtual DbSet<UserJoinChat> UserJoinChats { get; set; } = null!;
        public virtual DbSet<ChatRoom> ChatRooms { get; set; } = null!;
		public virtual DbSet<MangaAuthor> Authors { get; set; } = null!;
		public virtual DbSet<MangaArtist> Artists { get; set; } = null!;

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server = DESKTOP-RNGDEGO; Database = WebTruyenTranh_v3; MultipleActiveResultSets = True; Trusted_Connection = True;");
                //Server = ASUSTUF\\LESONHAI; Database = WebTruyenTranh_v2; MultipleActiveResultSets = True; Trusted_Connection = True;
                //"Data Source=SQL5111.site4now.net;Initial Catalog=db_aa1527_nafami;User Id=db_aa1527_nafami_admin;Password=hai09871825460

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //SeedRole(modelBuilder);

            modelBuilder.Ignore<IdentityUserToken<string>>();
            modelBuilder.Ignore<IdentityUserLogin<string>>();
            modelBuilder.Ignore<IdentityUserClaim<string>>();
            modelBuilder.Ignore<IdentityRoleClaim<string>>();

            modelBuilder.Entity<BoTruyen>(entity =>
            {
                entity.HasKey(e => e.MangaId);

                entity.ToTable("BoTruyen");

                entity.HasIndex(e => e.Id, "IX_BoTruyen_Id");

                entity.Property(e => e.MangaId)
                    .HasMaxLength(10)
                    .IsUnicode(false);


                entity.Property(e => e.MangaName).HasMaxLength(200);

                entity.HasOne(d => d.IdNavigation)
                    .WithMany(p => p.BoTruyens)
                    .HasForeignKey(d => d.Id)
                    .HasConstraintName("FK_Botruyen_UserAccount");
                entity.HasOne(d => d.TypeNavigation)
                    .WithMany(p => p.BoTruyens)
                    .HasForeignKey(d => d.Type)
                    .HasConstraintName("FK_BoTruyen_TypeManga");

                entity.HasMany(d => d.Genres)
                   .WithMany(p => p.Mangas)
                   .UsingEntity<Dictionary<string, object>>(
                       "TheLoaiBoTruyen",
                       l => l.HasOne<TheLoai>().WithMany().HasForeignKey("GenreId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_TheLoaiBoTruyen_TheLoai"),
                       r => r.HasOne<BoTruyen>().WithMany().HasForeignKey("MangaId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_TheLoaiBoTruyen_BoTruyen"),
                       j =>
                       {
                           j.HasKey("MangaId", "GenreId");

                           j.ToTable("TheLoaiBoTruyen");

                           j.HasIndex(new[] { "GenreId" }, "IX_TheLoaiBoTruyen_GenreId");

                           j.IndexerProperty<string>("MangaId").HasMaxLength(10).IsUnicode(false);
                       });
            });
            modelBuilder.Entity<RatingManga>(entity =>
            {
                entity.ToTable("RatingManga");

                entity.Property(e => e.Mangaid)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.Manga)
                    .WithMany(p => p.RatingMangas)
                    .HasForeignKey(d => d.Mangaid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RatingManga_BoTruyen");
            });
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasKey(e => new { e.IdUser, e.IdBotruyen });

                entity.HasOne(d => d.User)
                  .WithMany(p => p.bookmarks)
                  .HasForeignKey(d => d.IdUser)
                  .HasConstraintName("FK_Bookmark_User");
                entity.HasOne(d => d.BoTruyen)
                .WithMany(p => p.bookmarks)
                .HasForeignKey(d => d.IdBotruyen)
                .HasConstraintName("FK_Bookmark_Botruyen");

                entity.ToTable("Bookmark");
            });

            modelBuilder.Entity<TypeManga>(entity =>
            {
                entity.ToTable("TypeManga");

                entity.HasIndex(e => e.Id, "IX_TYPEMANGA");

                entity.Property(e => e.Name).HasMaxLength(100);
            });
            modelBuilder.Entity<ChapterImage>(entity =>
            {
                entity.HasKey(e => e.ImageId)
                    .HasName("PK_ChapterImage_1");

                entity.ToTable("ChapterImage");

                entity.HasIndex(e => e.ChapterId, "IX_ChapterImage_ChapterId");

                entity.Property(e => e.ChapterId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.Chapter)
                    .WithMany(p => p.ChapterImages)
                    .HasForeignKey(d => d.ChapterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChapterImage_ChuongTruyen1");
            });

            modelBuilder.Entity<ChuongTruyen>(entity =>
            {
                entity.HasKey(e => e.ChapterId)
                    .HasName("PK_ChuongTruyen_1");

                entity.Property(e => e.ChapterIndex)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.ToTable("ChuongTruyen");

                entity.HasIndex(e => e.MangaId, "IX_ChuongTruyen_MangaId");

                entity.Property(e => e.ChapterId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ChapterName).HasMaxLength(50);

                entity.Property(e => e.MangaId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.Manga)
                    .WithMany(p => p.ChuongTruyens)
                    .HasForeignKey(d => d.MangaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChuongTruyen_ChuongTruyen");
            });
            modelBuilder.Entity<BotruyenViewCount>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id, "IX_ViewCount_Id");

                entity.Property(e => e.Viewbydate)
                      .HasMaxLength(10)
                      .IsUnicode(false);
                entity.Property(e => e.Viewbymonth)
                      .HasMaxLength(10)
                      .IsUnicode(false);
                entity.Property(e => e.Viewbyyear)
                      .HasMaxLength(10)
                      .IsUnicode(false);

                entity.HasOne(d => d.botruyen)
                    .WithMany(p => p.BotruyenViewCounts)
                    .HasForeignKey(d => d.Id)
                    .HasConstraintName("FK_Botruyen_ViewCount");
                entity.ToTable("ViewCount");
            });

            modelBuilder.Entity<Role>(entity =>
            {   
                entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });
            modelBuilder.Entity<TheLoai>(entity =>
            {
                entity.HasKey(e => e.GenreId);

                entity.ToTable("TheLoai");

                entity.Property(e => e.GenresIdName).HasMaxLength(50);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });
            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.HasKey(r => new { r.UserId, r.RoleId });

                entity.ToTable("UserRoles");
            });
            modelBuilder.Entity<ThongbaoUser>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Id, "IX_ThongBao_User");

                entity.Property(e => e.dateTime).HasMaxLength(50);

                entity.Property(e => e.message).HasColumnType("NVARCHAR(MAX)").IsRequired();

                entity.HasOne(d => d.IdNavigation)
               .WithMany(p => p.Notification)
               .HasForeignKey(d => d.IdUser)
               .HasConstraintName("FK_ThongBaoUser_Users");

                entity.ToTable("ThongBaoUser");
            });
            modelBuilder.Entity<BinhLuan>(entity =>
            {
                entity.HasKey(e => e.IdComment)
                    .HasName("PK__BinhLuan__57C9AD582785D467");

                entity.ToTable("BinhLuan");

                entity.HasIndex(e => e.IdComment, "IX_Comment");

                entity.Property(e => e.ChapterId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MangaId)
                 .HasMaxLength(10)
                 .IsUnicode(false);

                entity.Property(e => e.IdUser).HasMaxLength(450);


                entity.HasOne(d => d.Manga)
                    .WithMany(p => p.BinhLuans)
                    .HasForeignKey(d => d.MangaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Commentn_Manga");

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.BinhLuans)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Commentn_User");
            });
            modelBuilder.Entity<ReplyComment>(entity =>
            {
                entity.HasKey(e => e.IdReply)
                    .HasName("PK__ReplyCom__EED5DB35404A40A4");

                entity.ToTable("ReplyComment");

                entity.HasIndex(e => e.IdReply, "IX_ReplyComment");

                entity.Property(e => e.IdComment).HasMaxLength(450);

                entity.Property(e => e.IdUserReply).HasMaxLength(450);

                entity.HasOne(d => d.IdCommentNavigation)
                    .WithMany(p => p.ReplyComments)
                    .HasForeignKey(d => d.IdComment)
                    .HasConstraintName("FK_Reply_Comment");

                entity.HasOne(d => d.IdUserReplyNavigation)
                    .WithMany(p => p.ReplyComments)
                    .HasForeignKey(d => d.IdUserReply)
                    .HasConstraintName("FK_Reply_User");
            });
        
            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(e => e.RoomId)
                    .HasName("PK__ChatRoom__32863939EEE78146");

                entity.ToTable("ChatRoom");

                entity.HasIndex(e => e.RoomId, "IX_RoomChat");

                entity.Property(e => e.RoomId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MangaId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.Manga)
                    .WithMany(p => p.ChatRooms)
                    .HasForeignKey(d => d.MangaId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Botruyen_RoomChat");
            });
            modelBuilder.Entity<Datachat>(entity =>
            {
                entity.HasKey(e => e.ChatId)
                    .HasName("PK__Datachat__A9FBE7C60DDBADDF");

                entity.ToTable("Datachat");

                entity.Property(e => e.RoomId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.UserJoinChat)
                    .WithMany(p => p.Datachats)
                    .HasForeignKey(d => new { d.RoomId, d.UserId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Datachat_UserJoinChat");
            });
            modelBuilder.Entity<UserJoinChat>(entity =>
            {
                entity.HasKey(e => new { e.RoomId, e.UserId })
                    .HasName("PK__UserJoin__E3FEB5FD75AD19C1");

                entity.ToTable("UserJoinChat");

                entity.Property(e => e.RoomId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.UserJoinChats)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoomChat_UserJoinChat");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserJoinChats)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_UserJoinChat");
            });
			modelBuilder.Entity<BoTruyen>()
	            .HasMany(b => b.MangaAuthors)
	            .WithMany(a => a.BoTruyens)
	            .UsingEntity(j => j.ToTable("BoTruyenMangaAuthors"));

			modelBuilder.Entity<BoTruyen>()
				.HasMany(b => b.MangaArtists)
				.WithMany(a => a.BoTruyens)
				.UsingEntity(j => j.ToTable("BoTruyenMangaArtists"));
			OnModelCreatingPartial(modelBuilder);
        }
        private static void SeedRole(ModelBuilder builder)
        {
            string[] datathelloai = new string[] { "Action", "Adventure", "Comedy", "Comic", "Doujinshi", "Drama", "Ecchi",
            "Fantasy","Full Color","Game","Gender Bender","Harem","Historical","Horror","Josei","Magic","Manhua","Manhwa","Martial Arts",
            "Mecha","Mystery","Romance","School Life","Sci-fi","Seinen","Shoujo","Shoujo Ai","Shounen","Shounen Ai","Slice of life","Sports",
            "Supernatural","Tragedy","Yuri","Webtoon"};
            for (int i = 0; i < datathelloai.Length; i++)
            {
                builder.Entity<TheLoai>().HasData(
                        new TheLoai() { GenreId = i + 1, GenresIdName = datathelloai[i] }
                        );
            }
            builder.Entity<Role>().HasData(
                new Role() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new Role() { Name = "Mod", ConcurrencyStamp = "2", NormalizedName = "Mod" },
                new Role() { Name = "User", ConcurrencyStamp = "3", NormalizedName = "User" },
                new Role() { Name = "Upload", ConcurrencyStamp = "4", NormalizedName = "Upload" }
                );
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
