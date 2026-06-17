using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentsReportIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE NONCLUSTERED INDEX IX_Payments_Status_CreatedAt_IncludeAmount
ON dbo.Payments (PaymentStatusId, CreatedAt)
INCLUDE (Amount);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP INDEX IF EXISTS IX_Payments_Status_CreatedAt_IncludeAmount
ON dbo.Payments;
");
        }
    }
}
