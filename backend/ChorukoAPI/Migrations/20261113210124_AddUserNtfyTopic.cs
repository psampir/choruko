using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChorukoAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNtfyTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NtfyTopic",
                table: "AspNetUsers",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_NtfyTopic",
                table: "AspNetUsers",
                column: "NtfyTopic",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_NtfyTopic",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NtfyTopic",
                table: "AspNetUsers");
        }
    }
}
