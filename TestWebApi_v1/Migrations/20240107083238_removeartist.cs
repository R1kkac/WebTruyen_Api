using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWebApi_v1.Migrations
{
    public partial class removeartist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MangaArtist",
                table: "BoTruyen");

            migrationBuilder.DropColumn(
                name: "MangaAuthor",
                table: "BoTruyen");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MangaArtist",
                table: "BoTruyen",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MangaAuthor",
                table: "BoTruyen",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
