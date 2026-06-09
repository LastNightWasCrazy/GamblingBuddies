using GamblingBuddies.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("reservations")]
        public async Task<IActionResult> GetReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Player)
                .Include(r => r.GameSession)
                    .ThenInclude(gs => gs.GameVariant)
                        .ThenInclude(gv => gv.Game)
                .Include(r => r.ReservationStatus)
                .Select(r => new
                {
                    r.ReservationId,
                    Player = r.Player != null ? r.Player.FirstName + " " + r.Player.LastName : "Brak",
                    PlayerEmail = r.Player != null ? r.Player.Email : "Brak",
                    Game = r.GameSession != null && r.GameSession.GameVariant != null && r.GameSession.GameVariant.Game != null ? r.GameSession.GameVariant.Game.Name : "Brak",
                    Variant = r.GameSession != null && r.GameSession.GameVariant != null ? r.GameSession.GameVariant.Name : "Brak",
                    r.ReservedAt,
                    Status = r.ReservationStatus != null ? r.ReservationStatus.Name : "Brak"
                })
                .ToListAsync();

            return Ok(reservations);
        }

        [HttpGet("reservations/{id}")]
        public async Task<IActionResult> GetReservation(int id)
        {
            try
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Player)
                    .Include(r => r.GameSession)
                        .ThenInclude(gs => gs.GameVariant)
                            .ThenInclude(gv => gv.Game)
                    .Include(r => r.GameSession)
                        .ThenInclude(gs => gs.GameTable)
                            .ThenInclude(gt => gt.Hall)
                    .Include(r => r.ReservationStatus)
                    .Include(r => r.ReservationSeats)
                        .ThenInclude(rs => rs.Seat)
                    .FirstOrDefaultAsync(r => r.ReservationId == id);

                if (reservation == null)
                    return NotFound(new { message = "Rezerwacja nie znaleziona" });

                var result = new
                {
                    reservation.ReservationId,
                    Player = reservation.Player != null ? new
                    {
                        reservation.Player.PlayerId,
                        reservation.Player.FirstName,
                        reservation.Player.LastName,
                        reservation.Player.Email,
                        reservation.Player.Phone
                    } : null,
                    GameSession = reservation.GameSession != null ? new
                    {
                        reservation.GameSession.GameSessionId,
                        Game = reservation.GameSession.GameVariant != null && reservation.GameSession.GameVariant.Game != null ? reservation.GameSession.GameVariant.Game.Name : "Brak",
                        Variant = reservation.GameSession.GameVariant != null ? reservation.GameSession.GameVariant.Name : "Brak",
                        reservation.GameSession.StartAt,
                        reservation.GameSession.EndAt,
                        Table = reservation.GameSession.GameTable != null ? reservation.GameSession.GameTable.TableNumber : 0,
                        Hall = reservation.GameSession.GameTable != null && reservation.GameSession.GameTable.Hall != null ? reservation.GameSession.GameTable.Hall.Name : "Brak"
                    } : null,
                    reservation.ReservedAt,
                    Status = reservation.ReservationStatus != null ? reservation.ReservationStatus.Name : "Brak",
                    Seats = reservation.ReservationSeats != null ? reservation.ReservationSeats.Select(rs => new
                    {
                        SeatId = rs.Seat != null ? rs.Seat.SeatId : 0,
                        SeatNumber = rs.Seat != null ? rs.Seat.SeatNumber : 0
                    }) : null
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentStatus)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Player)
                .Select(p => new
                {
                    p.PaymentId,
                    p.Amount,
                    Player = p.Reservation != null && p.Reservation.Player != null ? p.Reservation.Player.FirstName + " " + p.Reservation.Player.LastName : "Brak",
                    ReservationId = p.ReservationId,
                    Method = p.PaymentMethod != null ? p.PaymentMethod.Name : "Brak",
                    Status = p.PaymentStatus != null ? p.PaymentStatus.Name : "Brak",
                    p.CreatedAt,
                    p.PaidAt
                })
                .ToListAsync();

            return Ok(payments);
        }

        [HttpGet("payments/{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.PaymentMethod)
                    .Include(p => p.PaymentStatus)
                    .Include(p => p.Reservation)
                        .ThenInclude(r => r.Player)
                    .FirstOrDefaultAsync(p => p.PaymentId == id);

                if (payment == null)
                    return NotFound(new { message = "Płatność nie znaleziona" });

                var result = new
                {
                    payment.PaymentId,
                    payment.Amount,
                    Reservation = payment.Reservation != null ? new
                    {
                        payment.Reservation.ReservationId,
                        Player = payment.Reservation.Player != null ? payment.Reservation.Player.FirstName + " " + payment.Reservation.Player.LastName : "Brak",
                        PlayerEmail = payment.Reservation.Player != null ? payment.Reservation.Player.Email : "Brak"
                    } : null,
                    payment.PaymentMethodId,
                    Method = payment.PaymentMethod != null ? payment.PaymentMethod.Name : "Brak",
                    payment.PaymentStatusId,
                    Status = payment.PaymentStatus != null ? payment.PaymentStatus.Name : "Brak",
                    payment.CreatedAt,
                    payment.PaidAt
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("gamesessions")]
        public async Task<IActionResult> GetGameSessions()
        {
            var sessions = await _context.GameSessions
                .Include(gs => gs.GameVariant)
                    .ThenInclude(gv => gv.Game)
                .Include(gs => gs.GameTable)
                    .ThenInclude(gt => gt.Hall)
                .Include(gs => gs.SessionStatus)
                .Select(gs => new
                {
                    gs.GameSessionId,
                    Game = gs.GameVariant != null && gs.GameVariant.Game != null ? gs.GameVariant.Game.Name : "Brak",
                    Variant = gs.GameVariant != null ? gs.GameVariant.Name : "Brak",
                    Hall = gs.GameTable != null && gs.GameTable.Hall != null ? gs.GameTable.Hall.Name : "Brak",
                    TableNumber = gs.GameTable != null ? gs.GameTable.TableNumber : 0,
                    gs.StartAt,
                    gs.EndAt,
                    Status = gs.SessionStatus != null ? gs.SessionStatus.Name : "Brak"
                })
                .ToListAsync();

            return Ok(sessions);
        }

        [HttpGet("gamesessions/{id}")]
        public async Task<IActionResult> GetGameSession(int id)
        {
            try
            {
                var session = await _context.GameSessions
                    .Include(gs => gs.GameVariant)
                        .ThenInclude(gv => gv.Game)
                    .Include(gs => gs.GameTable)
                        .ThenInclude(gt => gt.Hall)
                    .Include(gs => gs.SessionStatus)
                    .FirstOrDefaultAsync(gs => gs.GameSessionId == id);

                if (session == null)
                    return NotFound(new { message = "Sesja nie znaleziona" });

                var result = new
                {
                    session.GameSessionId,
                    Game = session.GameVariant != null && session.GameVariant.Game != null ? session.GameVariant.Game.Name : "Brak",
                    Variant = session.GameVariant != null ? session.GameVariant.Name : "Brak",
                    RulesDescription = session.GameVariant != null ? session.GameVariant.RulesDescription : "Brak",
                    Hall = session.GameTable != null && session.GameTable.Hall != null ? session.GameTable.Hall.Name : "Brak",
                    TableNumber = session.GameTable != null ? session.GameTable.TableNumber : 0,
                    session.StartAt,
                    session.EndAt,
                    Status = session.SessionStatus != null ? session.SessionStatus.Name : "Brak"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("halls")]
        public async Task<IActionResult> GetHalls()
        {
            var halls = await _context.Halls
                .Where(h => h.IsActive)
                .Select(h => new
                {
                    h.HallId,
                    h.Name,
                    h.Description
                })
                .ToListAsync();

            return Ok(halls);
        }

        [HttpGet("tables")]
        public async Task<IActionResult> GetTables()
        {
            var tables = await _context.GameTables
                .Include(t => t.Hall)
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    t.GameTableId,
                    Hall = t.Hall != null ? t.Hall.Name : "Brak",
                    t.TableNumber,
                    t.MinPlayers,
                    t.MaxPlayers
                })
                .ToListAsync();

            return Ok(tables);
        }

        [HttpGet("games")]
        public async Task<IActionResult> GetGames()
        {
            var games = await _context.Games
                .Where(g => g.IsActive)
                .Select(g => new
                {
                    g.GameId,
                    g.Name,
                    g.Description,
                    g.GameCategoryId
                })
                .ToListAsync();

            return Ok(games);
        }

        [HttpGet("players")]
        public async Task<IActionResult> GetPlayers()
        {
            var players = await _context.Players
                .Select(p => new
                {
                    p.PlayerId,
                    p.FirstName,
                    p.LastName,
                    p.Email,
                    p.Phone,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(players);
        }
    }
}