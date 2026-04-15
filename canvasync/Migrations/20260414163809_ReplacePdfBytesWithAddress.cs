using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace canvasync.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePdfBytesWithAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfFileBytes",
                table: "Lectures");

            migrationBuilder.AddColumn<string>(
                name: "PdfFileAddress",
                table: "Lectures",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfFileAddress",
                table: "Lectures");

            migrationBuilder.AddColumn<byte[]>(
                name: "PdfFileBytes",
                table: "Lectures",
                type: "bytea",
                nullable: true);
        }
    }
}
