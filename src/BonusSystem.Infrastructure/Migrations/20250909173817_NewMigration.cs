using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TransactionReturn_OneTransactionType",
                schema: "bonus",
                table: "transactionReturns");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TransactionReturn_OneTransactionType",
                schema: "bonus",
                table: "transactionReturns",
                sql: "(\"BonusTransactionId\" IS NOT NULL AND \"FiatTransactionId\" IS NULL) \n                OR (\"BonusTransactionId\" IS NULL AND \"FiatTransactionId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TransactionReturn_OneTransactionType",
                schema: "bonus",
                table: "transactionReturns");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TransactionReturn_OneTransactionType",
                schema: "bonus",
                table: "transactionReturns",
                sql: "(\"BonusTransactionId\" IS NOT NULL AND \"FiatTransactionId\" IS NULL) \r\n                OR (\"BonusTransactionId\" IS NULL AND \"FiatTransactionId\" IS NOT NULL)");
        }
    }
}
