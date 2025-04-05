using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TKPM_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "8a16a1c6-77b2-4a7e-b5e1-8b9c2e8df845", null, "Anonymous", "ANONYMOUS" },
                    { "9d8c7b6a-5e4f-3d2c-1b0a-9f8e7d6c5b4a", null, "Premium", "PREMIUM" },
                    { "a7b8c9d0-e1f2-3a4b-5c6d-7e8f9a0b1c2d", null, "User", "USER" },
                    { "b3a2c1d0-e9f8-7d6c-5b4a-3e2f1c0d9a8b", null, "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8a16a1c6-77b2-4a7e-b5e1-8b9c2e8df845");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9d8c7b6a-5e4f-3d2c-1b0a-9f8e7d6c5b4a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a7b8c9d0-e1f2-3a4b-5c6d-7e8f9a0b1c2d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b3a2c1d0-e9f8-7d6c-5b4a-3e2f1c0d9a8b");
        }
    }
}
