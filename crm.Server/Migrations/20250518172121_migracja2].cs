using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crm.Server.Migrations
{
    /// <inheritdoc />
    public partial class migracja2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnrolledUserIds",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnrolledUserIds",
                table: "Courses");
        }
    }
}
