using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace canvasync.Migrations
{
    /// <inheritdoc />
    public partial class AddIsInProgressToLecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInProgress",
                table: "Lectures",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInProgress",
                table: "Lectures");
        }
    }
}
