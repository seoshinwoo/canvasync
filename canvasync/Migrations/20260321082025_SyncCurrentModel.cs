using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace canvasync.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInProgress",
                table: "Lectures");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInProgress",
                table: "Lectures",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
