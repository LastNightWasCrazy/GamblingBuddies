using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.SBD_DatabaseAuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SBD_DatabaseAuditLogs (
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
CREATE OR ALTER TRIGGER dbo.trg_Payments_DatabaseAudit
ON dbo.Payments
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.SBD_DatabaseAuditLogs
    (
        TableName,
        OperationType,
        RecordId,
        ExecutedBy,
        ExecutedAt
    )
    SELECT
        'Payments',
        CASE
            WHEN i.PaymentId IS NOT NULL AND d.PaymentId IS NULL THEN 'INSERT'
            WHEN i.PaymentId IS NOT NULL AND d.PaymentId IS NOT NULL THEN 'UPDATE'
            WHEN i.PaymentId IS NULL AND d.PaymentId IS NOT NULL THEN 'DELETE'
        END,
        COALESCE(i.PaymentId, d.PaymentId),
        SYSTEM_USER,
        GETDATE()
    FROM inserted i
    FULL OUTER JOIN deleted d
        ON i.PaymentId = d.PaymentId;
END
");
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_Employees_DatabaseAudit
ON dbo.Employees
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.SBD_DatabaseAuditLogs
    (
        TableName,
        OperationType,
        RecordId,
        ExecutedBy,
        ExecutedAt
    )
    SELECT
        'Employees',
        CASE
            WHEN i.EmployeeId IS NOT NULL AND d.EmployeeId IS NULL THEN 'INSERT'
            WHEN i.EmployeeId IS NOT NULL AND d.EmployeeId IS NOT NULL THEN 'UPDATE'
            WHEN i.EmployeeId IS NULL AND d.EmployeeId IS NOT NULL THEN 'DELETE'
        END,
        COALESCE(i.EmployeeId, d.EmployeeId),
        SYSTEM_USER,
        GETDATE()
    FROM inserted i
    FULL OUTER JOIN deleted d
        ON i.EmployeeId = d.EmployeeId;
END
");
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_Reservations_DatabaseAudit
ON dbo.Reservations
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.SBD_DatabaseAuditLogs
    (
        TableName,
        OperationType,
        RecordId,
        ExecutedBy,
        ExecutedAt
    )
    SELECT
        'Reservations',
        CASE
            WHEN i.ReservationId IS NOT NULL AND d.ReservationId IS NULL THEN 'INSERT'
            WHEN i.ReservationId IS NOT NULL AND d.ReservationId IS NOT NULL THEN 'UPDATE'
            WHEN i.ReservationId IS NULL AND d.ReservationId IS NOT NULL THEN 'DELETE'
        END,
        COALESCE(i.ReservationId, d.ReservationId),
        SYSTEM_USER,
        GETDATE()
    FROM inserted i
    FULL OUTER JOIN deleted d
        ON i.ReservationId = d.ReservationId;
END
");
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_RegistrationRequests_DatabaseAudit
ON dbo.RegistrationRequests
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.SBD_DatabaseAuditLogs
    (
        TableName,
        OperationType,
        RecordId,
        ExecutedBy,
        ExecutedAt
    )
    SELECT
        'RegistrationRequests',
        CASE
            WHEN i.RegistrationRequestId IS NOT NULL AND d.RegistrationRequestId IS NULL THEN 'INSERT'
            WHEN i.RegistrationRequestId IS NOT NULL AND d.RegistrationRequestId IS NOT NULL THEN 'UPDATE'
            WHEN i.RegistrationRequestId IS NULL AND d.RegistrationRequestId IS NOT NULL THEN 'DELETE'
        END,
        COALESCE(i.RegistrationRequestId, d.RegistrationRequestId),
        SYSTEM_USER,
        GETDATE()
    FROM inserted i
    FULL OUTER JOIN deleted d
        ON i.RegistrationRequestId = d.RegistrationRequestId;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS dbo.trg_RegistrationRequests_DatabaseAudit;");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS dbo.trg_Reservations_DatabaseAudit;");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS dbo.trg_Employees_DatabaseAudit;");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS dbo.trg_Payments_DatabaseAudit;");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS dbo.SBD_DatabaseAuditLogs;");
        }
    }
}
