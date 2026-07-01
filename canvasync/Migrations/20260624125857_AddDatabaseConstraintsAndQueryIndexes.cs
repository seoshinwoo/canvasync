using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace canvasync.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseConstraintsAndQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DELETE FROM ""DrawingData"" d
                  WHERE NOT EXISTS (
                      SELECT 1 FROM ""Lectures"" l WHERE l.""Id"" = d.""LectureId""
                  )
                  OR NOT EXISTS (
                      SELECT 1 FROM ""Members"" m WHERE m.""Id"" = d.""MemberId""
                  );");

            migrationBuilder.Sql(
                @"DELETE FROM ""DrawingData"" d
                  USING ""DrawingData"" newer
                  WHERE d.""LectureId"" = newer.""LectureId""
                    AND d.""MemberId"" = newer.""MemberId""
                    AND d.""Id"" < newer.""Id"";");

            migrationBuilder.Sql(
                @"WITH duplicated AS (
                      SELECT ""Id"",
                             row_number() OVER (PARTITION BY ""Code"" ORDER BY ""Id"") AS duplicate_order
                      FROM ""Lectures""
                  ),
                  targets AS (
                      SELECT ""Id"",
                             row_number() OVER (ORDER BY ""Id"") AS target_order
                      FROM duplicated
                      WHERE duplicate_order > 1
                  ),
                  candidate_codes AS (
                      SELECT lpad(code::text, 6, '0') AS code,
                             row_number() OVER (ORDER BY code) AS candidate_order
                      FROM generate_series(0, 999999) AS code
                      WHERE NOT EXISTS (
                          SELECT 1
                          FROM ""Lectures"" l
                          WHERE l.""Code"" = lpad(code::text, 6, '0')
                      )
                  ),
                  assignments AS (
                      SELECT targets.""Id"", candidate_codes.code
                      FROM targets
                      JOIN candidate_codes
                        ON candidate_codes.candidate_order = targets.target_order
                  )
                  UPDATE ""Lectures"" l
                  SET ""Code"" = assignments.code
                  FROM assignments
                  WHERE l.""Id"" = assignments.""Id"";");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Members",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Lectures",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Lectures",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Name",
                table: "Members",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lectures_Code",
                table: "Lectures",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrawingData_LectureId_MemberId",
                table: "DrawingData",
                columns: new[] { "LectureId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrawingData_MemberId",
                table: "DrawingData",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_DrawingData_Lectures_LectureId",
                table: "DrawingData",
                column: "LectureId",
                principalTable: "Lectures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrawingData_Members_MemberId",
                table: "DrawingData",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrawingData_Lectures_LectureId",
                table: "DrawingData");

            migrationBuilder.DropForeignKey(
                name: "FK_DrawingData_Members_MemberId",
                table: "DrawingData");

            migrationBuilder.DropIndex(
                name: "IX_Members_Name",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Lectures_Code",
                table: "Lectures");

            migrationBuilder.DropIndex(
                name: "IX_DrawingData_LectureId_MemberId",
                table: "DrawingData");

            migrationBuilder.DropIndex(
                name: "IX_DrawingData_MemberId",
                table: "DrawingData");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Members",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "Lectures",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Lectures",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6);
        }
    }
}
