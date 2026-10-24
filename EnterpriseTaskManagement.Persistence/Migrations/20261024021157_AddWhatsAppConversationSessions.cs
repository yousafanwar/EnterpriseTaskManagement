using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseTaskManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppConversationSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhatsAppConversationSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessPhoneNumberId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SenderWaId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Step = table.Column<int>(type: "int", nullable: false),
                    SelectedProjectId = table.Column<int>(type: "int", nullable: false),
                    SelectedProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppConversationSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppConversationSessions_BusinessPhoneNumberId_SenderWaId",
                table: "WhatsAppConversationSessions",
                columns: new[] { "BusinessPhoneNumberId", "SenderWaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhatsAppConversationSessions");
        }
    }
}
