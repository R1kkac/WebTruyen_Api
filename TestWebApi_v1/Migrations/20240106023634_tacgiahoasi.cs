using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWebApi_v1.Migrations
{
    public partial class tacgiahoasi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MangaArtist",
                columns: table => new
                {
                    MangaArtistId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlternateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MangaImage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaArtist", x => x.MangaArtistId);
                });

            migrationBuilder.CreateTable(
                name: "MangaAuthor",
                columns: table => new
                {
                    MangaAuthorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlternateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MangaImage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaAuthor", x => x.MangaAuthorId);
                });

            migrationBuilder.CreateTable(
                name: "BoTruyenMangaArtists",
                columns: table => new
                {
                    BoTruyensMangaId = table.Column<string>(type: "varchar(10)", nullable: false),
                    MangaArtistsMangaArtistId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoTruyenMangaArtists", x => new { x.BoTruyensMangaId, x.MangaArtistsMangaArtistId });
                    table.ForeignKey(
                        name: "FK_BoTruyenMangaArtists_BoTruyen_BoTruyensMangaId",
                        column: x => x.BoTruyensMangaId,
                        principalTable: "BoTruyen",
                        principalColumn: "MangaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoTruyenMangaArtists_MangaArtist_MangaArtistsMangaArtistId",
                        column: x => x.MangaArtistsMangaArtistId,
                        principalTable: "MangaArtist",
                        principalColumn: "MangaArtistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoTruyenMangaAuthors",
                columns: table => new
                {
                    BoTruyensMangaId = table.Column<string>(type: "varchar(10)", nullable: false),
                    MangaAuthorsMangaAuthorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoTruyenMangaAuthors", x => new { x.BoTruyensMangaId, x.MangaAuthorsMangaAuthorId });
                    table.ForeignKey(
                        name: "FK_BoTruyenMangaAuthors_BoTruyen_BoTruyensMangaId",
                        column: x => x.BoTruyensMangaId,
                        principalTable: "BoTruyen",
                        principalColumn: "MangaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoTruyenMangaAuthors_MangaAuthor_MangaAuthorsMangaAuthorId",
                        column: x => x.MangaAuthorsMangaAuthorId,
                        principalTable: "MangaAuthor",
                        principalColumn: "MangaAuthorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoTruyenMangaArtists_MangaArtistsMangaArtistId",
                table: "BoTruyenMangaArtists",
                column: "MangaArtistsMangaArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_BoTruyenMangaAuthors_MangaAuthorsMangaAuthorId",
                table: "BoTruyenMangaAuthors",
                column: "MangaAuthorsMangaAuthorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoTruyenMangaArtists");

            migrationBuilder.DropTable(
                name: "BoTruyenMangaAuthors");

            migrationBuilder.DropTable(
                name: "MangaArtist");

            migrationBuilder.DropTable(
                name: "MangaAuthor");
        }
    }
}
