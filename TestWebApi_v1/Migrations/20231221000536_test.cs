using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWebApi_v1.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TheLoai",
                columns: table => new
                {
                    GenreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GenresIdName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Info = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheLoai", x => x.GenreId);
                });

            migrationBuilder.CreateTable(
                name: "TypeManga",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeManga", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JoinDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoTruyen",
                columns: table => new
                {
                    MangaId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    MangaName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MangaDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MangaImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MangaAlternateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MangaAuthor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MangaArtist = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Dateupdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoTruyen", x => x.MangaId);
                    table.ForeignKey(
                        name: "FK_BoTruyen_TypeManga",
                        column: x => x.Type,
                        principalTable: "TypeManga",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Botruyen_UserAccount",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "KenhChatUser",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    TenPhong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NgayDong = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TinhTrang = table.Column<bool>(type: "bit", nullable: false),
                    NguoiTao = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KenhChatUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KenhChatUser_Users",
                        column: x => x.NguoiTao,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThongBaoUser",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdUser = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    seen = table.Column<bool>(type: "bit", nullable: false),
                    message = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    dateTime = table.Column<DateTime>(type: "datetime2", maxLength: 50, nullable: false),
                    target = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaoUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongBaoUser_Users",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookmark",
                columns: table => new
                {
                    IdUser = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdBotruyen = table.Column<string>(type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmark", x => new { x.IdUser, x.IdBotruyen });
                    table.ForeignKey(
                        name: "FK_Bookmark_Botruyen",
                        column: x => x.IdBotruyen,
                        principalTable: "BoTruyen",
                        principalColumn: "MangaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookmark_User",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChuongTruyen",
                columns: table => new
                {
                    ChapterId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ChapterName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChapterTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChapterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MangaId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ChapterIndex = table.Column<int>(type: "int", unicode: false, maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTruyen_1", x => x.ChapterId);
                    table.ForeignKey(
                        name: "FK_ChuongTruyen_ChuongTruyen",
                        column: x => x.MangaId,
                        principalTable: "BoTruyen",
                        principalColumn: "MangaId");
                });

            migrationBuilder.CreateTable(
                name: "RatingManga",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mangaid = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    NumberRating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingManga", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RatingManga_BoTruyen",
                        column: x => x.Mangaid,
                        principalTable: "BoTruyen",
                        principalColumn: "MangaId");
                });

            migrationBuilder.CreateTable(
                name: "TheLoaiBoTruyen",
                columns: table => new
                {
                    MangaId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheLoaiBoTruyen", x => new { x.MangaId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_TheLoaiBoTruyen_BoTruyen",
                        column: x => x.MangaId,
                        principalTable: "BoTruyen",
                        principalColumn: "MangaId");
                    table.ForeignKey(
                        name: "FK_TheLoaiBoTruyen_TheLoai",
                        column: x => x.GenreId,
                        principalTable: "TheLoai",
                        principalColumn: "GenreId");
                });

            migrationBuilder.CreateTable(
                name: "ViewCount",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(10)", nullable: false),
                    Viewbydate = table.Column<int>(type: "int", unicode: false, maxLength: 10, nullable: false),
                    Viewbymonth = table.Column<int>(type: "int", unicode: false, maxLength: 10, nullable: false),
                    Viewbyyear = table.Column<int>(type: "int", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewCount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Botruyen_ViewCount",
                        column: x => x.Id,
                        principalTable: "BoTruyen",
                        principalColumn: "MangaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User_RoomChat",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    JoinTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    OutTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IdUser = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IdChatRoom = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_RoomChat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_RoomChat_KenhChatUser",
                        column: x => x.IdChatRoom,
                        principalTable: "KenhChatUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_RoomChat_Users",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BinhLuan",
                columns: table => new
                {
                    IdComment = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdUser = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MangaId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ChapterId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    CommentData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Likecomment = table.Column<int>(type: "int", nullable: true),
                    Dislikecomment = table.Column<int>(type: "int", nullable: true),
                    DateComment = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BinhLuan__57C9AD582785D467", x => x.IdComment);
                    table.ForeignKey(
                        name: "FK_Commentn_Chapter",
                        column: x => x.ChapterId,
                        principalTable: "ChuongTruyen",
                        principalColumn: "ChapterId");
                    table.ForeignKey(
                        name: "FK_Commentn_Manga",
                        column: x => x.MangaId,
                        principalTable: "BoTruyen",
                        principalColumn: "MangaId");
                    table.ForeignKey(
                        name: "FK_Commentn_User",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChapterImage",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChapterId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterImage_1", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_ChapterImage_ChuongTruyen1",
                        column: x => x.ChapterId,
                        principalTable: "ChuongTruyen",
                        principalColumn: "ChapterId");
                });

            migrationBuilder.CreateTable(
                name: "Data_User_Chat",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    IdUserChatRoom = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Datetime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Data_User_Chat", x => x.id);
                    table.ForeignKey(
                        name: "FK_Data_User_Chat_User_RoomChat",
                        column: x => x.IdUserChatRoom,
                        principalTable: "User_RoomChat",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReplyComment",
                columns: table => new
                {
                    IdReply = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdComment = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IdUserReply = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Replydata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateReply = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ReplyCom__EED5DB35404A40A4", x => x.IdReply);
                    table.ForeignKey(
                        name: "FK_Reply_Comment",
                        column: x => x.IdComment,
                        principalTable: "BinhLuan",
                        principalColumn: "IdComment");
                    table.ForeignKey(
                        name: "FK_Reply_User",
                        column: x => x.IdUserReply,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_ChapterId",
                table: "BinhLuan",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_IdUser",
                table: "BinhLuan",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_BinhLuan_MangaId",
                table: "BinhLuan",
                column: "MangaId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment",
                table: "BinhLuan",
                column: "IdComment");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmark_IdBotruyen",
                table: "Bookmark",
                column: "IdBotruyen");

            migrationBuilder.CreateIndex(
                name: "IX_BoTruyen_Id",
                table: "BoTruyen",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BoTruyen_Type",
                table: "BoTruyen",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterImage_ChapterId",
                table: "ChapterImage",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTruyen_MangaId",
                table: "ChuongTruyen",
                column: "MangaId");

            migrationBuilder.CreateIndex(
                name: "IX_Data_User_Chat_IdUserChatRoom",
                table: "Data_User_Chat",
                column: "IdUserChatRoom");

            migrationBuilder.CreateIndex(
                name: "IX_KenhChatUser_NguoiTao",
                table: "KenhChatUser",
                column: "NguoiTao");

            migrationBuilder.CreateIndex(
                name: "IX_RatingManga_Mangaid",
                table: "RatingManga",
                column: "Mangaid");

            migrationBuilder.CreateIndex(
                name: "IX_ReplyComment",
                table: "ReplyComment",
                column: "IdReply");

            migrationBuilder.CreateIndex(
                name: "IX_ReplyComment_IdComment",
                table: "ReplyComment",
                column: "IdComment");

            migrationBuilder.CreateIndex(
                name: "IX_ReplyComment_IdUserReply",
                table: "ReplyComment",
                column: "IdUserReply");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "([NormalizedName] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_TheLoaiBoTruyen_GenreId",
                table: "TheLoaiBoTruyen",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_User",
                table: "ThongBaoUser",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaoUser_IdUser",
                table: "ThongBaoUser",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_TYPEMANGA",
                table: "TypeManga",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoomChat_IdChatRoom",
                table: "User_RoomChat",
                column: "IdChatRoom");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoomChat_IdUser",
                table: "User_RoomChat",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "([NormalizedUserName] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_ViewCount_Id",
                table: "ViewCount",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookmark");

            migrationBuilder.DropTable(
                name: "ChapterImage");

            migrationBuilder.DropTable(
                name: "Data_User_Chat");

            migrationBuilder.DropTable(
                name: "RatingManga");

            migrationBuilder.DropTable(
                name: "ReplyComment");

            migrationBuilder.DropTable(
                name: "TheLoaiBoTruyen");

            migrationBuilder.DropTable(
                name: "ThongBaoUser");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "ViewCount");

            migrationBuilder.DropTable(
                name: "User_RoomChat");

            migrationBuilder.DropTable(
                name: "BinhLuan");

            migrationBuilder.DropTable(
                name: "TheLoai");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "KenhChatUser");

            migrationBuilder.DropTable(
                name: "ChuongTruyen");

            migrationBuilder.DropTable(
                name: "BoTruyen");

            migrationBuilder.DropTable(
                name: "TypeManga");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
