using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class AddRptView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW rpt.view_UsersWithRoles
AS
SELECT
    u.SystemUserId,
    u.Login,
    u.Email,
    u.IsActive,
    u.IsApproved,
    ISNULL(r.Name, 'Brak roli') AS RoleName
FROM SystemUsers u
LEFT JOIN UserRoles ur
    ON u.SystemUserId = ur.SystemUserId
LEFT JOIN RoleDictionaries r
    ON ur.RoleDictionaryId = r.RoleDictionaryId;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS rpt.view_UsersWithRoles;");
        }
    }
}
