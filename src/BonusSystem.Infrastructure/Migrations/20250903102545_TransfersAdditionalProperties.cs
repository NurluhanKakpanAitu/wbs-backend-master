using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BonusSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransfersAdditionalProperties : Migration
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
