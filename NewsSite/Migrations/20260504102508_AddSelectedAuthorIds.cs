using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsSite.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedAuthorIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedAuthIds",
                table: "NewsletterPreferences",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedAuthIds",
                table: "NewsletterPreferences");
        }
    }
}
