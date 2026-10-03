using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogSync",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    FirstImported = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastChecked = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Added = table.Column<int>(type: "INTEGER", nullable: false),
                    Updated = table.Column<int>(type: "INTEGER", nullable: false),
                    Removed = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogSync", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogSync_Kind",
                table: "CatalogSync",
                column: "Kind",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogSync");
        }
    }
}
