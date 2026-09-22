using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BB84.IPTV.M3U.Editor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "Logos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DownloadedAt",
                table: "Logos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ETag",
                table: "Logos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Logos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalPath",
                table: "Logos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Logos_Url",
                table: "Logos",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Logos_Url",
                table: "Logos");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "Logos");

            migrationBuilder.DropColumn(
                name: "DownloadedAt",
                table: "Logos");

            migrationBuilder.DropColumn(
                name: "ETag",
                table: "Logos");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Logos");

            migrationBuilder.DropColumn(
                name: "LocalPath",
                table: "Logos");
        }
    }
}