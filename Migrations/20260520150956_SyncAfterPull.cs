using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class SyncAfterPull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_SessionStatusDictionaries_SessionStatusDictionarySessionStatusId",
                table: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_SessionStatusDictionarySessionStatusId",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "SessionStatusDictionarySessionStatusId",
                table: "GameSessions");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_SessionStatusId",
                table: "GameSessions",
                column: "SessionStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_SessionStatusDictionaries_SessionStatusId",
                table: "GameSessions",
                column: "SessionStatusId",
                principalTable: "SessionStatusDictionaries",
                principalColumn: "SessionStatusId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_SessionStatusDictionaries_SessionStatusId",
                table: "GameSessions");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_SessionStatusId",
                table: "GameSessions");

            migrationBuilder.AddColumn<int>(
                name: "SessionStatusDictionarySessionStatusId",
                table: "GameSessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_SessionStatusDictionarySessionStatusId",
                table: "GameSessions",
                column: "SessionStatusDictionarySessionStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_SessionStatusDictionaries_SessionStatusDictionarySessionStatusId",
                table: "GameSessions",
                column: "SessionStatusDictionarySessionStatusId",
                principalTable: "SessionStatusDictionaries",
                principalColumn: "SessionStatusId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
