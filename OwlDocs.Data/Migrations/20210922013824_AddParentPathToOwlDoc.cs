using Microsoft.EntityFrameworkCore.Migrations;

namespace OwlDocs.Data.Migrations
{
    public partial class AddParentPathToOwlDoc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentPath",
                table: "OwlDocuments",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentPath",
                table: "OwlDocuments");
        }
    }
}
