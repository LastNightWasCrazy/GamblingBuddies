using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class AddSBDTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.RegistrationRequestStatusHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RegistrationRequestStatusHistory (
        HistoryId INT IDENTITY PRIMARY KEY,
        RegistrationRequestId INT NOT NULL,
        OldStatus NVARCHAR(100) NULL,
        NewStatus NVARCHAR(100) NULL,
        ChangedBy NVARCHAR(128) NOT NULL,
        ChangedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
");

            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_RegistrationRequest_StatusHistory
ON dbo.RegistrationRequests
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.RegistrationRequestStatusHistory
    (
        RegistrationRequestId,
        OldStatus,
        NewStatus,
        ChangedBy,
        ChangedAt
    )
    SELECT
        i.RegistrationRequestId,
        d.Status,
        i.Status,
        SYSTEM_USER,
        GETDATE()
    FROM inserted i
    JOIN deleted d
        ON i.RegistrationRequestId = d.RegistrationRequestId
    WHERE ISNULL(i.Status, '') <> ISNULL(d.Status, '');
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.PaymentArchive', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentArchive (
        PaymentArchiveId INT IDENTITY PRIMARY KEY,
        PaymentId INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        ArchivedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        ArchivedBy NVARCHAR(128) NOT NULL
    );
END
");

            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER dbo.trg_Archive_CompletedPayments
ON dbo.Payments
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.PaymentArchive
    (
        PaymentId,
        Amount,
        ArchivedAt,
        ArchivedBy
    )
    SELECT
        i.PaymentId,
        i.Amount,
        GETDATE(),
        SYSTEM_USER
    FROM inserted i
    JOIN deleted d
        ON i.PaymentId = d.PaymentId
    WHERE i.PaymentStatusId <> d.PaymentStatusId
      AND i.PaymentStatusId IN (
          SELECT PaymentStatusId
          FROM PaymentStatuses
          WHERE Name IN ('Completed', 'Paid', 'Zakończone', 'Opłacone')
      )
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.PaymentArchive pa
          WHERE pa.PaymentId = i.PaymentId
      );
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS dbo.trg_Archive_CompletedPayments;");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS dbo.trg_RegistrationRequest_StatusHistory;");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS dbo.PaymentArchive;");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS dbo.RegistrationRequestStatusHistory;");
        }
    }
}
