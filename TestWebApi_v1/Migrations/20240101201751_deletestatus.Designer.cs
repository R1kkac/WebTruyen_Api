﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TestWebApi_v1.Models;

#nullable disable

namespace TestWebApi_v1.Migrations
{
    [DbContext(typeof(WebTruyenTranh_v2Context))]
    [Migration("20240101201751_deletestatus")]
    partial class deletestatus
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.BinhLuan", b =>
                {
                    b.Property<string>("IdComment")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ChapterId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<string>("CommentData")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("DateComment")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("Dislikecomment")
                        .HasColumnType("int");

                    b.Property<string>("IdUser")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("Likecomment")
                        .HasColumnType("int");

                    b.Property<string>("MangaId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.HasKey("IdComment")
                        .HasName("PK__BinhLuan__57C9AD582785D467");

                    b.HasIndex("IdUser");

                    b.HasIndex("MangaId");

                    b.HasIndex(new[] { "IdComment" }, "IX_Comment");

                    b.ToTable("BinhLuan", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.Bookmark", b =>
                {
                    b.Property<string>("IdUser")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("IdBotruyen")
                        .HasColumnType("varchar(10)");

                    b.HasKey("IdUser", "IdBotruyen");

                    b.HasIndex("IdBotruyen");

                    b.ToTable("Bookmark", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.BoTruyen", b =>
                {
                    b.Property<string>("MangaId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<DateTime?>("Dateupdate")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("DeleteStatus")
                        .HasColumnType("bit");

                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("MangaAlternateName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MangaArtist")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("MangaAuthor")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("MangaDetails")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MangaImage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MangaName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime?>("MarkedAsDeletedDate")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("Status")
                        .HasColumnType("bit");

                    b.Property<int?>("Type")
                        .HasColumnType("int");

                    b.HasKey("MangaId");

                    b.HasIndex("Type");

                    b.HasIndex(new[] { "Id" }, "IX_BoTruyen_Id");

                    b.ToTable("BoTruyen", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.BotruyenViewCount", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(10)");

                    b.Property<int>("Viewbydate")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("int");

                    b.Property<int>("Viewbymonth")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("int");

                    b.Property<int>("Viewbyyear")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Id" }, "IX_ViewCount_Id");

                    b.ToTable("ViewCount", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ChapterImage", b =>
                {
                    b.Property<int>("ImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ImageId"), 1L, 1);

                    b.Property<string>("ChapterId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<string>("ImageName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageUl")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ImageId")
                        .HasName("PK_ChapterImage_1");

                    b.HasIndex(new[] { "ChapterId" }, "IX_ChapterImage_ChapterId");

                    b.ToTable("ChapterImage", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ChatRoom", b =>
                {
                    b.Property<string>("RoomId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<DateTimeOffset?>("EndTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("MangaId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<bool?>("Status")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("TimeStart")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("RoomId")
                        .HasName("PK__ChatRoom__32863939EEE78146");

                    b.HasIndex("MangaId");

                    b.HasIndex(new[] { "RoomId" }, "IX_RoomChat");

                    b.ToTable("ChatRoom", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ChuongTruyen", b =>
                {
                    b.Property<string>("ChapterId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<DateTime>("ChapterDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ChapterIndex")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("int");

                    b.Property<string>("ChapterName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ChapterTitle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MangaId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.HasKey("ChapterId")
                        .HasName("PK_ChuongTruyen_1");

                    b.HasIndex(new[] { "MangaId" }, "IX_ChuongTruyen_MangaId");

                    b.ToTable("ChuongTruyen", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.Datachat", b =>
                {
                    b.Property<int>("ChatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ChatId"), 1L, 1);

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoomId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<DateTimeOffset?>("TimeChat")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("ChatId")
                        .HasName("PK__Datachat__A9FBE7C60DDBADDF");

                    b.HasIndex("RoomId", "UserId");

                    b.ToTable("Datachat", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.RatingManga", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Mangaid")
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("NumberRating")
                        .HasColumnType("int");

                    b.Property<double>("Rating")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("Mangaid");

                    b.ToTable("RatingManga", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ReplyComment", b =>
                {
                    b.Property<string>("IdReply")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTimeOffset?>("DateReply")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("IdComment")
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("IdUserReply")
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Replydata")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdReply")
                        .HasName("PK__ReplyCom__EED5DB35404A40A4");

                    b.HasIndex("IdComment");

                    b.HasIndex("IdUserReply");

                    b.HasIndex(new[] { "IdReply" }, "IX_ReplyComment");

                    b.ToTable("ReplyComment", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.Role", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "NormalizedName" }, "RoleNameIndex")
                        .IsUnique()
                        .HasFilter("([NormalizedName] IS NOT NULL)");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.TheLoai", b =>
                {
                    b.Property<int>("GenreId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("GenreId"), 1L, 1);

                    b.Property<string>("GenresIdName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Info")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("GenreId");

                    b.ToTable("TheLoai", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ThongbaoUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("IdUser")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("dateTime")
                        .HasMaxLength(50)
                        .HasColumnType("datetime2");

                    b.Property<string>("message")
                        .IsRequired()
                        .HasColumnType("NVARCHAR(MAX)");

                    b.Property<bool>("seen")
                        .HasColumnType("bit");

                    b.Property<string>("target")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("IdUser");

                    b.HasIndex(new[] { "Id" }, "IX_ThongBao_User");

                    b.ToTable("ThongBaoUser", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.TypeManga", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Id" }, "IX_TYPEMANGA");

                    b.ToTable("TypeManga", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("Avatar")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConcurrencyStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("JoinDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "NormalizedEmail" }, "EmailIndex");

                    b.HasIndex(new[] { "NormalizedUserName" }, "UserNameIndex")
                        .IsUnique()
                        .HasFilter("([NormalizedUserName] IS NOT NULL)");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.UserJoinChat", b =>
                {
                    b.Property<string>("RoomId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool?>("Status")
                        .HasColumnType("bit");

                    b.HasKey("RoomId", "UserId")
                        .HasName("PK__UserJoin__E3FEB5FD75AD19C1");

                    b.HasIndex("UserId");

                    b.ToTable("UserJoinChat", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.UserRoles", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles", (string)null);
                });

            modelBuilder.Entity("TheLoaiBoTruyen", b =>
                {
                    b.Property<string>("MangaId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("GenreId")
                        .HasColumnType("int");

                    b.HasKey("MangaId", "GenreId");

                    b.HasIndex(new[] { "GenreId" }, "IX_TheLoaiBoTruyen_GenreId");

                    b.ToTable("TheLoaiBoTruyen", (string)null);
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.BinhLuan", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.User", "IdUserNavigation")
                        .WithMany("BinhLuans")
                        .HasForeignKey("IdUser")
                        .IsRequired()
                        .HasConstraintName("FK_Commentn_User");

                    b.HasOne("TestWebApi_v1.Models.DbContext.BoTruyen", "Manga")
                        .WithMany("BinhLuans")
                        .HasForeignKey("MangaId")
                        .IsRequired()
                        .HasConstraintName("FK_Commentn_Manga");

                    b.Navigation("IdUserNavigation");

                    b.Navigation("Manga");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.Bookmark", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.BoTruyen", "BoTruyen")
                        .WithMany("bookmarks")
                        .HasForeignKey("IdBotruyen")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Bookmark_Botruyen");

                    b.HasOne("TestWebApi_v1.Models.DbContext.User", "User")
                        .WithMany("bookmarks")
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Bookmark_User");

                    b.Navigation("BoTruyen");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.BoTruyen", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.User", "IdNavigation")
                        .WithMany("BoTruyens")
                        .HasForeignKey("Id")
                        .HasConstraintName("FK_Botruyen_UserAccount");

                    b.HasOne("TestWebApi_v1.Models.DbContext.TypeManga", "TypeNavigation")
                        .WithMany("BoTruyens")
                        .HasForeignKey("Type")
                        .HasConstraintName("FK_BoTruyen_TypeManga");

                    b.Navigation("IdNavigation");

                    b.Navigation("TypeNavigation");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.BotruyenViewCount", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.BoTruyen", "botruyen")
                        .WithMany("BotruyenViewCounts")
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Botruyen_ViewCount");

                    b.Navigation("botruyen");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ChapterImage", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.ChuongTruyen", "Chapter")
                        .WithMany("ChapterImages")
                        .HasForeignKey("ChapterId")
                        .IsRequired()
                        .HasConstraintName("FK_ChapterImage_ChuongTruyen1");

                    b.Navigation("Chapter");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ChatRoom", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.BoTruyen", "Manga")
                        .WithMany("ChatRooms")
                        .HasForeignKey("MangaId")
                        .IsRequired()
                        .HasConstraintName("FK_Botruyen_RoomChat");

                    b.Navigation("Manga");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ChuongTruyen", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.BoTruyen", "Manga")
                        .WithMany("ChuongTruyens")
                        .HasForeignKey("MangaId")
                        .IsRequired()
                        .HasConstraintName("FK_ChuongTruyen_ChuongTruyen");

                    b.Navigation("Manga");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.Datachat", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.UserJoinChat", "UserJoinChat")
                        .WithMany("Datachats")
                        .HasForeignKey("RoomId", "UserId")
                        .IsRequired()
                        .HasConstraintName("FK_Datachat_UserJoinChat");

                    b.Navigation("UserJoinChat");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.RatingManga", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.BoTruyen", "Manga")
                        .WithMany("RatingMangas")
                        .HasForeignKey("Mangaid")
                        .IsRequired()
                        .HasConstraintName("FK_RatingManga_BoTruyen");

                    b.Navigation("Manga");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ReplyComment", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.BinhLuan", "IdCommentNavigation")
                        .WithMany("ReplyComments")
                        .HasForeignKey("IdComment")
                        .HasConstraintName("FK_Reply_Comment");

                    b.HasOne("TestWebApi_v1.Models.DbContext.User", "IdUserReplyNavigation")
                        .WithMany("ReplyComments")
                        .HasForeignKey("IdUserReply")
                        .HasConstraintName("FK_Reply_User");

                    b.Navigation("IdCommentNavigation");

                    b.Navigation("IdUserReplyNavigation");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ThongbaoUser", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.User", "IdNavigation")
                        .WithMany("Notification")
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_ThongBaoUser_Users");

                    b.Navigation("IdNavigation");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.UserJoinChat", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.ChatRoom", "Room")
                        .WithMany("UserJoinChats")
                        .HasForeignKey("RoomId")
                        .IsRequired()
                        .HasConstraintName("FK_RoomChat_UserJoinChat");

                    b.HasOne("TestWebApi_v1.Models.DbContext.User", "User")
                        .WithMany("UserJoinChats")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK_User_UserJoinChat");

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.UserRoles", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.Role", "role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TestWebApi_v1.Models.DbContext.User", "user")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("role");

                    b.Navigation("user");
                });

            modelBuilder.Entity("TheLoaiBoTruyen", b =>
                {
                    b.HasOne("TestWebApi_v1.Models.DbContext.TheLoai", null)
                        .WithMany()
                        .HasForeignKey("GenreId")
                        .IsRequired()
                        .HasConstraintName("FK_TheLoaiBoTruyen_TheLoai");

                    b.HasOne("TestWebApi_v1.Models.DbContext.BoTruyen", null)
                        .WithMany()
                        .HasForeignKey("MangaId")
                        .IsRequired()
                        .HasConstraintName("FK_TheLoaiBoTruyen_BoTruyen");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.BinhLuan", b =>
                {
                    b.Navigation("ReplyComments");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.BoTruyen", b =>
                {
                    b.Navigation("BinhLuans");

                    b.Navigation("BotruyenViewCounts");

                    b.Navigation("ChatRooms");

                    b.Navigation("ChuongTruyens");

                    b.Navigation("RatingMangas");

                    b.Navigation("bookmarks");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ChatRoom", b =>
                {
                    b.Navigation("UserJoinChats");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.ChuongTruyen", b =>
                {
                    b.Navigation("ChapterImages");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.Role", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.TypeManga", b =>
                {
                    b.Navigation("BoTruyens");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.User", b =>
                {
                    b.Navigation("BinhLuans");

                    b.Navigation("BoTruyens");

                    b.Navigation("Notification");

                    b.Navigation("ReplyComments");

                    b.Navigation("UserJoinChats");

                    b.Navigation("UserRoles");

                    b.Navigation("bookmarks");
                });

            modelBuilder.Entity("TestWebApi_v1.Models.DbContext.UserJoinChat", b =>
                {
                    b.Navigation("Datachats");
                });
#pragma warning restore 612, 618
        }
    }
}