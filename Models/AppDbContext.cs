using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<RoleDictionary> RoleDictionaries { get; set; }
        public DbSet<EmployeePositionDictionary> EmployeePositionDictionaries { get; set; }
        public DbSet<EmployeeStatusDictionary> EmployeeStatusDictionaries { get; set; }
        public DbSet<SystemUser> SystemUsers { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationSeat> ReservationSeats { get; set; }
        public DbSet<ReservationStatusDictionary> ReservationStatusDictionaries { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<PaymentReport> PaymentReports { get; set; }
        public DbSet<PaymentStatusDictionary> PaymentStatuses { get; set; }
        public DbSet<PaymentMethodDictionary> PaymentMethods { get; set; }
        public DbSet<EmployeeAssignment> EmployeeAssignments { get; set; }
        public DbSet<WorkShift> WorkShifts { get; set; }
        public DbSet<ReportDefinition> ReportDefinitions { get; set; }
        public DbSet<ReportExecution> ReportExecutions { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<GameTable> GameTables { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameVariant> GameVariants { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<HallTypeDictionary> HallTypeDictionaries { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentFile> DocumentFiles { get; set; }
        public DbSet<GameCategoryDictionary> GameCategoryDictionaries { get; set; }
        public DbSet<SessionStatusDictionary> SessionStatusDictionaries { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}