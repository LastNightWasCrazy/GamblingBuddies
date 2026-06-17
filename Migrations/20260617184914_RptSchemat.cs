using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamblingBuddies.Migrations
{
    /// <inheritdoc />
    public partial class RptSchemat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'rpt')
BEGIN
    EXEC('CREATE SCHEMA rpt');
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'rpt')
BEGIN
    EXEC('DROP SCHEMA rpt');
END
");
        }
    }
}
