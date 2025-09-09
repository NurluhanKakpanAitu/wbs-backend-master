using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_bonus",
                schema: "bonus",
                table: "bonus");

            migrationBuilder.RenameTable(
                name: "bonus",
                schema: "bonus",
                newName: "category",
                newSchema: "bonus");

            migrationBuilder.RenameIndex(
                name: "IX_bonus_Id",
                schema: "bonus",
                table: "category",
                newName: "IX_category_Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_category",
                schema: "bonus",
                table: "category",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_category",
                schema: "bonus",
                table: "category");

            migrationBuilder.RenameTable(
                name: "category",
                schema: "bonus",
                newName: "bonus",
                newSchema: "bonus");

            migrationBuilder.RenameIndex(
                name: "IX_category_Id",
                schema: "bonus",
                table: "bonus",
                newName: "IX_bonus_Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bonus",
                schema: "bonus",
                table: "bonus",
                column: "Id");
        }
    }
}
