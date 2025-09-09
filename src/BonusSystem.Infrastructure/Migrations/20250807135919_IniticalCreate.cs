using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BonusSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IniticalCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bonus");

            migrationBuilder.CreateTable(
                name: "bonus",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bonus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BIN = table.Column<string>(type: "text", nullable: false),
                    INN = table.Column<string>(type: "text", nullable: false),
                    Number_of_contract = table.Column<string>(type: "text", nullable: false),
                    Date_of_contract = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Region = table.Column<int>(type: "integer", maxLength: 12, nullable: false),
                    City = table.Column<int>(type: "integer", maxLength: 12, nullable: false),
                    BonusBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    FiatBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    OriginalBonusBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    InitialBonusBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FrontendId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stores",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessTypeId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MallId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    City = table.Column<int>(type: "integer", maxLength: 100, nullable: false),
                    Region = table.Column<int>(type: "integer", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Floor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Row = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    WorkingHours = table.Column<string>(type: "text", nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TypeOfbusiness = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SellerIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    CompanyEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    FrontendId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stores_companies_CompanyEntityId",
                        column: x => x.CompanyEntityId,
                        principalSchema: "bonus",
                        principalTable: "companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_stores_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "bonus",
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    City = table.Column<int>(type: "integer", maxLength: 100, nullable: false),
                    Region = table.Column<int>(type: "integer", maxLength: 100, nullable: false),
                    INN = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    BonusBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    FiatBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationCode = table.Column<string>(type: "text", nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PincodeSet = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FrontendId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "bonus",
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "numeric", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_users_RecipientId",
                        column: x => x.RecipientId,
                        principalSchema: "bonus",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_users_SenderId",
                        column: x => x.SenderId,
                        principalSchema: "bonus",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fiatTransactions",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FrontendId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: true),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: true),
                    BonusAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FiatCashBackRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FiatTransactionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FiatCashBackAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CommissionPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiatTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fiatTransactions_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "bonus",
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fiatTransactions_stores_StoreId",
                        column: x => x.StoreId,
                        principalSchema: "bonus",
                        principalTable: "stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_fiatTransactions_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "bonus",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_users_RecipientId",
                        column: x => x.RecipientId,
                        principalSchema: "bonus",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "transactions",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: true),
                    BonusAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CommissionPercent = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transactions_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "bonus",
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transactions_stores_StoreId",
                        column: x => x.StoreId,
                        principalSchema: "bonus",
                        principalTable: "stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_transactions_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "bonus",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactionReturns",
                schema: "bonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BonusTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    FiatTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactionReturns", x => x.Id);
                    table.CheckConstraint("CK_TransactionReturn_OneTransactionType", "(\"BonusTransactionId\" IS NOT NULL AND \"FiatTransactionId\" IS NULL) \n                OR (\"BonusTransactionId\" IS NULL AND \"FiatTransactionId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_transactionReturns_fiatTransactions_FiatTransactionId",
                        column: x => x.FiatTransactionId,
                        principalSchema: "bonus",
                        principalTable: "fiatTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactionReturns_transactions_BonusTransactionId",
                        column: x => x.BonusTransactionId,
                        principalSchema: "bonus",
                        principalTable: "transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactionReturns_users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalSchema: "bonus",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactionReturns_users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalSchema: "bonus",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_RecipientId",
                schema: "bonus",
                table: "Transfers",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SenderId",
                schema: "bonus",
                table: "Transfers",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_bonus_Id",
                schema: "bonus",
                table: "bonus",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_companies_Name",
                schema: "bonus",
                table: "companies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_companies_Status",
                schema: "bonus",
                table: "companies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_fiatTransactions_CompanyId",
                schema: "bonus",
                table: "fiatTransactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_fiatTransactions_Status",
                schema: "bonus",
                table: "fiatTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_fiatTransactions_StoreId",
                schema: "bonus",
                table: "fiatTransactions",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_fiatTransactions_Timestamp",
                schema: "bonus",
                table: "fiatTransactions",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_fiatTransactions_UserId",
                schema: "bonus",
                table: "fiatTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_CreatedAt",
                schema: "bonus",
                table: "notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_IsRead",
                schema: "bonus",
                table: "notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_RecipientId",
                schema: "bonus",
                table: "notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_Type",
                schema: "bonus",
                table: "notifications",
                column: "Type");

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

            migrationBuilder.CreateIndex(
                name: "IX_stores_CategoryId",
                schema: "bonus",
                table: "stores",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_stores_CompanyEntityId",
                schema: "bonus",
                table: "stores",
                column: "CompanyEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_stores_CompanyId",
                schema: "bonus",
                table: "stores",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_stores_Name",
                schema: "bonus",
                table: "stores",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_stores_Status",
                schema: "bonus",
                table: "stores",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_transactionReturns_ApprovedByUserId",
                schema: "bonus",
                table: "transactionReturns",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_transactionReturns_BonusTransactionId",
                schema: "bonus",
                table: "transactionReturns",
                column: "BonusTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactionReturns_FiatTransactionId",
                schema: "bonus",
                table: "transactionReturns",
                column: "FiatTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactionReturns_RequestedByUserId",
                schema: "bonus",
                table: "transactionReturns",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_transactionReturns_Status",
                schema: "bonus",
                table: "transactionReturns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_CompanyId",
                schema: "bonus",
                table: "transactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_Status",
                schema: "bonus",
                table: "transactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_StoreId",
                schema: "bonus",
                table: "transactions",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_Timestamp",
                schema: "bonus",
                table: "transactions",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_Type",
                schema: "bonus",
                table: "transactions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_UserId",
                schema: "bonus",
                table: "transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_CompanyId",
                schema: "bonus",
                table: "users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                schema: "bonus",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Role",
                schema: "bonus",
                table: "users",
                column: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transfers",
                schema: "bonus");

            migrationBuilder.DropTable(
                name: "bonus",
                schema: "bonus");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "bonus");

            migrationBuilder.DropTable(
                name: "store_sellers",
                schema: "bonus");

            migrationBuilder.DropTable(
                name: "transactionReturns",
                schema: "bonus");

            migrationBuilder.DropTable(
                name: "fiatTransactions",
                schema: "bonus");

            migrationBuilder.DropTable(
                name: "transactions",
                schema: "bonus");

            migrationBuilder.DropTable(
                name: "stores",
                schema: "bonus");

            migrationBuilder.DropTable(
                name: "users",
                schema: "bonus");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "bonus");
        }
    }
}
