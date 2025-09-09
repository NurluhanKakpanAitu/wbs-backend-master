using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FiatTransactionRelatedBonus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedBonusTransactionId",
                schema: "bonus",
                table: "fiatTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_fiatTransactions_RelatedBonusTransactionId",
                schema: "bonus",
                table: "fiatTransactions",
                column: "RelatedBonusTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_fiatTransactions_transactions_RelatedBonusTransactionId",
                schema: "bonus",
                table: "fiatTransactions",
                column: "RelatedBonusTransactionId",
                principalSchema: "bonus",
                principalTable: "transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fiatTransactions_transactions_RelatedBonusTransactionId",
                schema: "bonus",
                table: "fiatTransactions");

            migrationBuilder.DropIndex(
                name: "IX_fiatTransactions_RelatedBonusTransactionId",
                schema: "bonus",
                table: "fiatTransactions");

            migrationBuilder.DropColumn(
                name: "RelatedBonusTransactionId",
                schema: "bonus",
                table: "fiatTransactions");
        }
    }
}
