using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class AddRptFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER FUNCTION rpt.fn_UserRoleStats()
RETURNS TABLE
AS
RETURN
(
    SELECT
        ISNULL(r.Name, 'Brak roli') AS RoleName,
        COUNT(u.SystemUserId) AS UsersCount
    FROM SystemUsers u
    LEFT JOIN UserRoles ur
        ON u.SystemUserId = ur.SystemUserId
    LEFT JOIN RoleDictionaries r
        ON ur.RoleDictionaryId = r.RoleDictionaryId
    GROUP BY r.Name
);
");
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS rpt.fn_UserRoleStats;");
           
        }
    }
}
