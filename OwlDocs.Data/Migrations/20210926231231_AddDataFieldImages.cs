using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OwlDocs.Data.Migrations
{
    public partial class AddDataFieldImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Data",
                table: "OwlDocuments",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "OwlDocuments");
        }
    }
}
