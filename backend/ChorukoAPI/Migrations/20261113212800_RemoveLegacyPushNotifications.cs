using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChorukoAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyPushNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PushSubscriptions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", nullable: false),
                    HouseholdTaskId = table.Column<string>(type: "varchar(36)", nullable: true),
                    InvitationId = table.Column<string>(type: "varchar(36)", nullable: true),
                    ReceiverId = table.Column<string>(type: "varchar(36)", nullable: false),
                    SettlementId = table.Column<string>(type: "varchar(36)", nullable: true),
                    Message = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    NotificationType = table.Column<byte>(type: "smallint", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_HouseholdTasks_HouseholdTaskId",
                        column: x => x.HouseholdTaskId,
                        principalTable: "HouseholdTasks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AuthKey = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Endpoint = table.Column<string>(type: "text", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    P256dhKey = table.Column<string>(type: "text", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_HouseholdTaskId",
                table: "Notifications",
                column: "HouseholdTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_InvitationId",
                table: "Notifications",
                column: "InvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReceiverId",
                table: "Notifications",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SettlementId",
                table: "Notifications",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptions_Endpoint",
                table: "PushSubscriptions",
                column: "Endpoint",
                unique: true);
        }
    }
}
