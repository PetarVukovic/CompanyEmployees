using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CompanyEmployees.Migrations
{
    /// <inheritdoc />
    public partial class AddedRolesToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "db84b050-63c8-4840-a449-cab817395010", "8eeb6f2b-0610-4f89-ba2b-6746438c6ff9", "Administrator", "ADMINISTRATOR" },
                    { "e5f34591-36f4-4891-bc52-0d29d41f7b7d", "817a3ffc-3807-43cd-a8ef-5ce066c8a151", "Manager", "MANAGER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "db84b050-63c8-4840-a449-cab817395010");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e5f34591-36f4-4891-bc52-0d29d41f7b7d");
        }
    }
}
