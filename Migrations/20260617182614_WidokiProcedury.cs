using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class WidokiProcedury : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER VIEW view_UsersWithRoles
AS
SELECT
    u.SystemUserId,
    u.Login,
    u.Email,
    r.Name AS RoleName
FROM SystemUsers u
JOIN UserRoles ur
    ON u.SystemUserId = ur.SystemUserId
JOIN RoleDictionaries r
    ON ur.RoleDictionaryId = r.RoleDictionaryId;
");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE proc_GetUsersWithRoles
AS
BEGIN
    SELECT *
    FROM view_UsersWithRoles;
END
");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE proc_GetActiveUsers
AS
BEGIN
    SELECT
        SystemUserId,
        Login,
        Email,
        IsActive,
        IsApproved
    FROM SystemUsers
    WHERE IsActive = 1;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS proc_GetActiveUsers;");
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS proc_GetUsersWithRoles;");
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS view_UsersWithRoles;");
        }
    }
}
