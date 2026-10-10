using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseTaskManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiTaskFeedbackAndSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeedbackImagePublicId",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackImageUrl",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFeedback",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TaskSkills",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskSkills", x => new { x.TaskId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_TaskSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskSkills_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskSkills_SkillId",
                table: "TaskSkills",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskSkills");

            migrationBuilder.DropColumn(
                name: "FeedbackImagePublicId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "FeedbackImageUrl",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "OriginalFeedback",
                table: "Tasks");
        }
    }
}
