using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideSiteIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Guides_Site",
                table: "Guides",
                column: "Site");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Guides_Site",
                table: "Guides");
        }
    }
}
