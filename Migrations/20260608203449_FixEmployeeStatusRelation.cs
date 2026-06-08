using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class FixEmployeeStatusRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeStatusDictionaries_EmployeeStatusDictionaryId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Halls_HallTypeDictionaries_HallTypeDictionaryHallTypeId",
                table: "Halls");

            migrationBuilder.DropIndex(
                name: "IX_Halls_HallTypeDictionaryHallTypeId",
                table: "Halls");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeStatusDictionaryId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HallTypeDictionaryHallTypeId",
                table: "Halls");

            migrationBuilder.DropColumn(
                name: "EmployeePositionDictionaryId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmployeeStatusDictionaryId",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Games",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Halls_HallTypeId",
                table: "Halls",
                column: "HallTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeStatusId",
                table: "Employees",
                column: "EmployeeStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeStatusDictionaries_EmployeeStatusId",
                table: "Employees",
                column: "EmployeeStatusId",
                principalTable: "EmployeeStatusDictionaries",
                principalColumn: "EmployeeStatusDictionaryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Halls_HallTypeDictionaries_HallTypeId",
                table: "Halls",
                column: "HallTypeId",
                principalTable: "HallTypeDictionaries",
                principalColumn: "HallTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeStatusDictionaries_EmployeeStatusId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Halls_HallTypeDictionaries_HallTypeId",
                table: "Halls");

            migrationBuilder.DropIndex(
                name: "IX_Halls_HallTypeId",
                table: "Halls");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeStatusId",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "HallTypeDictionaryHallTypeId",
                table: "Halls",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeePositionDictionaryId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeStatusDictionaryId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Halls_HallTypeDictionaryHallTypeId",
                table: "Halls",
                column: "HallTypeDictionaryHallTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeStatusDictionaryId",
                table: "Employees",
                column: "EmployeeStatusDictionaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeStatusDictionaries_EmployeeStatusDictionaryId",
                table: "Employees",
                column: "EmployeeStatusDictionaryId",
                principalTable: "EmployeeStatusDictionaries",
                principalColumn: "EmployeeStatusDictionaryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Halls_HallTypeDictionaries_HallTypeDictionaryHallTypeId",
                table: "Halls",
                column: "HallTypeDictionaryHallTypeId",
                principalTable: "HallTypeDictionaries",
                principalColumn: "HallTypeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
