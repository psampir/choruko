using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChorukoAPI.Migrations
{
    /// <inheritdoc />
    public partial class InvitationTableFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecieverEmail",
                table: "Invitations",
                newName: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_ReceiverId",
                table: "Invitations",
                column: "ReceiverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_AspNetUsers_ReceiverId",
                table: "Invitations",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_AspNetUsers_ReceiverId",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_ReceiverId",
                table: "Invitations");

            migrationBuilder.RenameColumn(
                name: "ReceiverId",
                table: "Invitations",
                newName: "RecieverEmail");
        }
    }
}
