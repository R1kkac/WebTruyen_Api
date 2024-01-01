using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWebApi_v1.Migrations
{
    public partial class deletestatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeleteStatus",
                table: "BoTruyen",
                type: "bit",
                nullable: true);


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteStatus",
                table: "BoTruyen");


        }
    }
}
