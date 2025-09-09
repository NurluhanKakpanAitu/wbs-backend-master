using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "bonus",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommissionProcessed",
                schema: "bonus",
                table: "transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "malls",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    WorkHours = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<int>(type: "integer", nullable: false),
                    Region = table.Column<int>(type: "integer", nullable: false),
                    FrontendId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_malls", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "malls",
                schema: "bonus");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "bonus",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsCommissionProcessed",
                schema: "bonus",
                table: "transactions");
        }
    }
}
