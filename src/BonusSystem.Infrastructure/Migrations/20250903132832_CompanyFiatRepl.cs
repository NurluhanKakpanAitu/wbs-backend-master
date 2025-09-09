using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanyFiatRepl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_users_RecipientId",
                schema: "bonus",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_users_SenderId",
                schema: "bonus",
                table: "Transfers");

            migrationBuilder.CreateTable(
                name: "fiatReplenishmentCompanyBalances",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiatReplenishmentCompanyBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fiatReplenishmentCompanyBalances_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "bonus",
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fiatReplenishmentCompanyBalances_CompanyId",
                schema: "bonus",
                table: "fiatReplenishmentCompanyBalances",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_users_RecipientId",
                schema: "bonus",
                table: "Transfers",
                column: "RecipientId",
                principalSchema: "bonus",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_users_SenderId",
                schema: "bonus",
                table: "Transfers",
                column: "SenderId",
                principalSchema: "bonus",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_users_RecipientId",
                schema: "bonus",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_users_SenderId",
                schema: "bonus",
                table: "Transfers");

            migrationBuilder.DropTable(
                name: "fiatReplenishmentCompanyBalances",
                schema: "bonus");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_users_RecipientId",
                schema: "bonus",
                table: "Transfers",
                column: "RecipientId",
                principalSchema: "bonus",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_users_SenderId",
                schema: "bonus",
                table: "Transfers",
                column: "SenderId",
                principalSchema: "bonus",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
