using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class AddDmlAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.SBD_AuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SBD_AuditLogs (
        AuditLogId INT IDENTITY PRIMARY KEY,
        TableName NVARCHAR(100) NOT NULL,
        OperationType NVARCHAR(20) NOT NULL,
        RecordId INT NULL,
        ExecutedBy NVARCHAR(128) NOT NULL,
        ExecutedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
");
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_SystemUsers_Audit
ON dbo.SystemUsers
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.SBD_AuditLogs
    (
        TableName,
        OperationType,
        RecordId,
        ExecutedBy,
        ExecutedAt
    )
    SELECT
        'SystemUsers',
        CASE
            WHEN i.SystemUserId IS NOT NULL AND d.SystemUserId IS NULL THEN 'INSERT'
            WHEN i.SystemUserId IS NOT NULL AND d.SystemUserId IS NOT NULL THEN 'UPDATE'
            WHEN i.SystemUserId IS NULL AND d.SystemUserId IS NOT NULL THEN 'DELETE'
        END,
        COALESCE(i.SystemUserId, d.SystemUserId),
        SYSTEM_USER,
        GETDATE()
    FROM inserted i
    FULL OUTER JOIN deleted d
        ON i.SystemUserId = d.SystemUserId;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS dbo.trg_SystemUsers_Audit;");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS dbo.SBD_AuditLogs;");
        }
    }
}
