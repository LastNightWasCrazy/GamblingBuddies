using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        //
        public DbSet<SystemUser> SystemUsers { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Player> Players { get; set; }

        //
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationSeat> ReservationSeats { get; set; }
        public DbSet<ReservationStatusDictionary> ReservationStatusDictionaries { get; set; }

        //Wojtek doda
        public DbSet<Hall> Halls { get; set; }
        public DbSet<GameTable> GameTables { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameVariant> GameVariants { get; set; }
        public DbSet<GameSession> GameSessions { get; set; }
    }
}