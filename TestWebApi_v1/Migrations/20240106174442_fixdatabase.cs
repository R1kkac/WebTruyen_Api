using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWebApi_v1.Migrations
{
    public partial class fixdatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoTruyenMangaArtists_MangaArtist_MangaArtistsMangaArtistId",
                table: "BoTruyenMangaArtists");

            migrationBuilder.DropForeignKey(
                name: "FK_BoTruyenMangaAuthors_MangaAuthor_MangaAuthorsMangaAuthorId",
                table: "BoTruyenMangaAuthors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MangaAuthor",
                table: "MangaAuthor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MangaArtist",
                table: "MangaArtist");

            migrationBuilder.RenameTable(
                name: "MangaAuthor",
                newName: "Authors");

            migrationBuilder.RenameTable(
                name: "MangaArtist",
                newName: "Artists");

            migrationBuilder.RenameColumn(
                name: "MangaImage",
                table: "Authors",
                newName: "AuthorImage");

            migrationBuilder.RenameColumn(
                name: "MangaImage",
                table: "Artists",
                newName: "ArtistImage");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Authors",
                table: "Authors",
                column: "MangaAuthorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Artists",
                table: "Artists",
                column: "MangaArtistId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoTruyenMangaArtists_Artists_MangaArtistsMangaArtistId",
                table: "BoTruyenMangaArtists",
                column: "MangaArtistsMangaArtistId",
                principalTable: "Artists",
                principalColumn: "MangaArtistId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BoTruyenMangaAuthors_Authors_MangaAuthorsMangaAuthorId",
                table: "BoTruyenMangaAuthors",
                column: "MangaAuthorsMangaAuthorId",
                principalTable: "Authors",
                principalColumn: "MangaAuthorId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoTruyenMangaArtists_Artists_MangaArtistsMangaArtistId",
                table: "BoTruyenMangaArtists");

            migrationBuilder.DropForeignKey(
                name: "FK_BoTruyenMangaAuthors_Authors_MangaAuthorsMangaAuthorId",
                table: "BoTruyenMangaAuthors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Authors",
                table: "Authors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Artists",
                table: "Artists");

            migrationBuilder.RenameTable(
                name: "Authors",
                newName: "MangaAuthor");

            migrationBuilder.RenameTable(
                name: "Artists",
                newName: "MangaArtist");

            migrationBuilder.RenameColumn(
                name: "AuthorImage",
                table: "MangaAuthor",
                newName: "MangaImage");

            migrationBuilder.RenameColumn(
                name: "ArtistImage",
                table: "MangaArtist",
                newName: "MangaImage");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MangaAuthor",
                table: "MangaAuthor",
                column: "MangaAuthorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MangaArtist",
                table: "MangaArtist",
                column: "MangaArtistId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoTruyenMangaArtists_MangaArtist_MangaArtistsMangaArtistId",
                table: "BoTruyenMangaArtists",
                column: "MangaArtistsMangaArtistId",
                principalTable: "MangaArtist",
                principalColumn: "MangaArtistId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BoTruyenMangaAuthors_MangaAuthor_MangaAuthorsMangaAuthorId",
                table: "BoTruyenMangaAuthors",
                column: "MangaAuthorsMangaAuthorId",
                principalTable: "MangaAuthor",
                principalColumn: "MangaAuthorId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
