using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class AddRptProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE rpt.proc_GetUsersWithRoles
AS
BEGIN
    SELECT *
    FROM rpt.view_UsersWithRoles;
END
");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE rpt.proc_GetEmployeePositionStats
AS
BEGIN
    SELECT *
    FROM rpt.fn_EmployeePositionStats();
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS rpt.proc_GetEmployeePositionStats;");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS rpt.proc_GetUsersWithRoles;");
        }
    }
}
