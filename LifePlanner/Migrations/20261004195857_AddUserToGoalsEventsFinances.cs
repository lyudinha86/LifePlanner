using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifePlanner.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToGoalsEventsFinances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PlannerEvents",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Goals",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "FinancialTransactions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PlannerEvents_UserId",
                table: "PlannerEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId",
                table: "Goals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_UserId",
                table: "FinancialTransactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_AspNetUsers_UserId",
                table: "FinancialTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_AspNetUsers_UserId",
                table: "Goals",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlannerEvents_AspNetUsers_UserId",
                table: "PlannerEvents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_AspNetUsers_UserId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Goals_AspNetUsers_UserId",
                table: "Goals");

            migrationBuilder.DropForeignKey(
                name: "FK_PlannerEvents_AspNetUsers_UserId",
                table: "PlannerEvents");

            migrationBuilder.DropIndex(
                name: "IX_PlannerEvents_UserId",
                table: "PlannerEvents");

            migrationBuilder.DropIndex(
                name: "IX_Goals_UserId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_UserId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PlannerEvents");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FinancialTransactions");
        }
    }
}
