using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreSellerAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "store_sellers",
                schema: "bonus");

            migrationBuilder.DropColumn(
                name: "SellerIds",
                schema: "bonus",
                table: "stores");

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                schema: "bonus",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_StoreId",
                schema: "bonus",
                table: "users",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_users_stores_StoreId",
                schema: "bonus",
                table: "users",
                column: "StoreId",
                principalSchema: "bonus",
                principalTable: "stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_stores_StoreId",
                schema: "bonus",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_StoreId",
                schema: "bonus",
                table: "users");

            migrationBuilder.DropColumn(
                name: "StoreId",
                schema: "bonus",
                table: "users");

            migrationBuilder.AddColumn<List<Guid>>(
                name: "SellerIds",
                schema: "bonus",
                table: "stores",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "store_sellers",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    FrontendId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_store_sellers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_store_sellers_stores_StoreId",
                        column: x => x.StoreId,
                        principalSchema: "bonus",
                        principalTable: "stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_store_sellers_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "bonus",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_store_sellers_StoreId_UserId",
                schema: "bonus",
                table: "store_sellers",
                columns: new[] { "StoreId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_store_sellers_UserId",
                schema: "bonus",
                table: "store_sellers",
                column: "UserId");
        }
    }
}
