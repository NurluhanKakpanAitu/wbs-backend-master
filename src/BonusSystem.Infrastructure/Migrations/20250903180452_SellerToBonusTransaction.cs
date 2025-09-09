using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SellerToBonusTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SellerId",
                schema: "bonus",
                table: "transactions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellerId",
                schema: "bonus",
                table: "transactions");
        }
    }
}
