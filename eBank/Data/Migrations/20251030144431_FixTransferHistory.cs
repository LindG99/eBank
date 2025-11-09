using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eBank.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixTransferHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TransferHistories_FromAccountId",
                table: "TransferHistories",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferHistories_ToAccountId",
                table: "TransferHistories",
                column: "ToAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferHistories_BankAccounts_FromAccountId",
                table: "TransferHistories",
                column: "FromAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferHistories_BankAccounts_ToAccountId",
                table: "TransferHistories",
                column: "ToAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransferHistories_BankAccounts_FromAccountId",
                table: "TransferHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferHistories_BankAccounts_ToAccountId",
                table: "TransferHistories");

            migrationBuilder.DropIndex(
                name: "IX_TransferHistories_FromAccountId",
                table: "TransferHistories");

            migrationBuilder.DropIndex(
                name: "IX_TransferHistories_ToAccountId",
                table: "TransferHistories");
        }
    }
}
