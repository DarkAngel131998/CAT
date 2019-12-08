using Microsoft.EntityFrameworkCore.Migrations;

namespace CAT.Migrations
{
    public partial class updateDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StatusPreviousAnswer",
                table: "Exam",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusPreviousAnswer",
                table: "Exam");
        }
    }
}
