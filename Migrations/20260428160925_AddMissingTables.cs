using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Payments_PaymentId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_Reservations_ReservationId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_SystemUsers_CreatedByUserId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentFile_Document_DocumentId",
                table: "DocumentFile");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameCategoryDictionary_GameCategoryId",
                table: "Games");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameCategoryDictionary",
                table: "GameCategoryDictionary");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentFile",
                table: "DocumentFile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Document",
                table: "Document");

            migrationBuilder.RenameTable(
                name: "GameCategoryDictionary",
                newName: "GameCategoryDictionaries");

            migrationBuilder.RenameTable(
                name: "DocumentFile",
                newName: "DocumentFiles");

            migrationBuilder.RenameTable(
                name: "Document",
                newName: "Documents");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentFile_DocumentId",
                table: "DocumentFiles",
                newName: "IX_DocumentFiles_DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_ReservationId",
                table: "Documents",
                newName: "IX_Documents_ReservationId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_PaymentId",
                table: "Documents",
                newName: "IX_Documents_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_CreatedByUserId",
                table: "Documents",
                newName: "IX_Documents_CreatedByUserId");

            migrationBuilder.AddColumn<int>(
                name: "HallTypeDictionaryHallTypeId",
                table: "Halls",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionStatusDictionarySessionStatusId",
                table: "GameSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameCategoryDictionaries",
                table: "GameCategoryDictionaries",
                column: "GameCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentFiles",
                table: "DocumentFiles",
                column: "DocumentFileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: "Documents",
                column: "DocumentId");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemUserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_SystemUsers_SystemUserId",
                        column: x => x.SystemUserId,
                        principalTable: "SystemUsers",
                        principalColumn: "SystemUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HallTypeDictionaries",
                columns: table => new
                {
                    HallTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallTypeDictionaries", x => x.HallTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SessionStatusDictionaries",
                columns: table => new
                {
                    SessionStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionStatusDictionaries", x => x.SessionStatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Halls_HallTypeDictionaryHallTypeId",
                table: "Halls",
                column: "HallTypeDictionaryHallTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_SessionStatusDictionarySessionStatusId",
                table: "GameSessions",
                column: "SessionStatusDictionarySessionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SystemUserId",
                table: "AuditLogs",
                column: "SystemUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentFiles_Documents_DocumentId",
                table: "DocumentFiles",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Payments_PaymentId",
                table: "Documents",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Reservations_ReservationId",
                table: "Documents",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "ReservationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_SystemUsers_CreatedByUserId",
                table: "Documents",
                column: "CreatedByUserId",
                principalTable: "SystemUsers",
                principalColumn: "SystemUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_GameCategoryDictionaries_GameCategoryId",
                table: "Games",
                column: "GameCategoryId",
                principalTable: "GameCategoryDictionaries",
                principalColumn: "GameCategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GameSessions_SessionStatusDictionaries_SessionStatusDictionarySessionStatusId",
                table: "GameSessions",
                column: "SessionStatusDictionarySessionStatusId",
                principalTable: "SessionStatusDictionaries",
                principalColumn: "SessionStatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Halls_HallTypeDictionaries_HallTypeDictionaryHallTypeId",
                table: "Halls",
                column: "HallTypeDictionaryHallTypeId",
                principalTable: "HallTypeDictionaries",
                principalColumn: "HallTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentFiles_Documents_DocumentId",
                table: "DocumentFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Payments_PaymentId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Reservations_ReservationId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_SystemUsers_CreatedByUserId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameCategoryDictionaries_GameCategoryId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_GameSessions_SessionStatusDictionaries_SessionStatusDictionarySessionStatusId",
                table: "GameSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Halls_HallTypeDictionaries_HallTypeDictionaryHallTypeId",
                table: "Halls");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "HallTypeDictionaries");

            migrationBuilder.DropTable(
                name: "SessionStatusDictionaries");

            migrationBuilder.DropIndex(
                name: "IX_Halls_HallTypeDictionaryHallTypeId",
                table: "Halls");

            migrationBuilder.DropIndex(
                name: "IX_GameSessions_SessionStatusDictionarySessionStatusId",
                table: "GameSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameCategoryDictionaries",
                table: "GameCategoryDictionaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: "Documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentFiles",
                table: "DocumentFiles");

            migrationBuilder.DropColumn(
                name: "HallTypeDictionaryHallTypeId",
                table: "Halls");

            migrationBuilder.DropColumn(
                name: "SessionStatusDictionarySessionStatusId",
                table: "GameSessions");

            migrationBuilder.RenameTable(
                name: "GameCategoryDictionaries",
                newName: "GameCategoryDictionary");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "Document");

            migrationBuilder.RenameTable(
                name: "DocumentFiles",
                newName: "DocumentFile");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_ReservationId",
                table: "Document",
                newName: "IX_Document_ReservationId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_PaymentId",
                table: "Document",
                newName: "IX_Document_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_CreatedByUserId",
                table: "Document",
                newName: "IX_Document_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentFiles_DocumentId",
                table: "DocumentFile",
                newName: "IX_DocumentFile_DocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameCategoryDictionary",
                table: "GameCategoryDictionary",
                column: "GameCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Document",
                table: "Document",
                column: "DocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentFile",
                table: "DocumentFile",
                column: "DocumentFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Payments_PaymentId",
                table: "Document",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "PaymentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Reservations_ReservationId",
                table: "Document",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "ReservationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_SystemUsers_CreatedByUserId",
                table: "Document",
                column: "CreatedByUserId",
                principalTable: "SystemUsers",
                principalColumn: "SystemUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentFile_Document_DocumentId",
                table: "DocumentFile",
                column: "DocumentId",
                principalTable: "Document",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_GameCategoryDictionary_GameCategoryId",
                table: "Games",
                column: "GameCategoryId",
                principalTable: "GameCategoryDictionary",
                principalColumn: "GameCategoryId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
