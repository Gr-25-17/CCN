using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsSite.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Newsletters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    SelectedCategoryIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArticlesPerCategory = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    EditorChoiceCount = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    CustomHtmlHeader = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomHtmlFooter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduledSendTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecipientCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Newsletters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Newsletters_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Newsletters_CreatedByUserId",
                table: "Newsletters",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Newsletters_Status",
                table: "Newsletters",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Newsletters_CreatedAt",
                table: "Newsletters",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Newsletters");
        }
    }
}
