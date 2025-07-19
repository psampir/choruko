using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChorukoAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeHouseholdIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HouseholdId",
                table: "AspNetUsers",
                type: "varchar(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(36)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "HouseholdId",
                table: "AspNetUsers",
                type: "varchar(36)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldNullable: true);
        }
    }
}
