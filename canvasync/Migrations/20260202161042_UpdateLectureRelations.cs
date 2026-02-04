using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace canvasync.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLectureRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HostMemberId",
                table: "Lectures",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    MemberType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LectureMember",
                columns: table => new
                {
                    JoinedLecturesId = table.Column<string>(type: "text", nullable: false),
                    MembersId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureMember", x => new { x.JoinedLecturesId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_LectureMember_Lectures_JoinedLecturesId",
                        column: x => x.JoinedLecturesId,
                        principalTable: "Lectures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LectureMember_Members_MembersId",
                        column: x => x.MembersId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lectures_HostMemberId",
                table: "Lectures",
                column: "HostMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LectureMember_MembersId",
                table: "LectureMember",
                column: "MembersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_Members_HostMemberId",
                table: "Lectures",
                column: "HostMemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_Members_HostMemberId",
                table: "Lectures");

            migrationBuilder.DropTable(
                name: "LectureMember");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Lectures_HostMemberId",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "HostMemberId",
                table: "Lectures");
        }
    }
}
