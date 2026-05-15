using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsSite.Migrations
{
    /// <inheritdoc />
    public partial class AddUnsubscribeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUnsubscribed",
                table: "NewsletterPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UnsubscribeReason",
                table: "NewsletterPreferences",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnsubscribedAt",
                table: "NewsletterPreferences",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UnsubscribeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: true),
                    UnsubscribedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    WasReactivated = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReactivatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsubscribeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnsubscribeLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnsubscribeLogs_UnsubscribedAt",
                table: "UnsubscribeLogs",
                column: "UnsubscribedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UnsubscribeLogs_UserId",
                table: "UnsubscribeLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnsubscribeLogs");

            migrationBuilder.DropColumn(
                name: "IsUnsubscribed",
                table: "NewsletterPreferences");

            migrationBuilder.DropColumn(
                name: "UnsubscribeReason",
                table: "NewsletterPreferences");

            migrationBuilder.DropColumn(
                name: "UnsubscribedAt",
                table: "NewsletterPreferences");
        }
    }
}
