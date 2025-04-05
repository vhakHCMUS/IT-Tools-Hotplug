using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TKPM_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLikedToolTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserLikedTools",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ToolId = table.Column<int>(type: "INTEGER", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLikedTools", x => new { x.UserId, x.ToolId });
                    table.ForeignKey(
                        name: "FK_UserLikedTools_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLikedTools_Tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "Tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserLikedTools_ToolId",
                table: "UserLikedTools",
                column: "ToolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLikedTools");
        }
    }
}
