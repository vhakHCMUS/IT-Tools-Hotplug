using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TKPM_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPremiumTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPremiums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPremiums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPremiums_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPremiums_ExpireDate",
                table: "UserPremiums",
                column: "ExpireDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserPremiums_UserId",
                table: "UserPremiums",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPremiums");
        }
    }
}
