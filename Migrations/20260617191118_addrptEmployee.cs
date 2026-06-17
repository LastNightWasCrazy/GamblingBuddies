using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class addrptEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER FUNCTION rpt.fn_EmployeePositionStats()
RETURNS TABLE
AS
RETURN
(
    SELECT
        ep.Name AS PositionName,
        COUNT(e.EmployeeId) AS EmployeesCount
    FROM Employees e
    LEFT JOIN EmployeePositionDictionaries ep
        ON e.PositionId = ep.EmployeePositionDictionaryId
    GROUP BY ep.Name
);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS rpt.fn_EmployeePositionStats;");
        }
    }
}
