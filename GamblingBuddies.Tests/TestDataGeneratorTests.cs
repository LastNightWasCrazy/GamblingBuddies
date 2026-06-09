using GamblingBuddies.Models;

namespace GamblingBuddies.Tests
{
    public class TestDataGeneratorTests
    {
        [Fact]
        public void GenerateRandomPlayers_ReturnsExpectedNumberOfPlayers()
        {
            var players = TestDataGenerator.GenerateRandomPlayers(20);

            Assert.Equal(20, players.Count);
        }

        [Fact]
        public void GenerateRandomPlayers_GeneratesPlayersWithEmail()
        {
            var players = TestDataGenerator.GenerateRandomPlayers(10);

            Assert.All(players, player =>
            {
                Assert.False(string.IsNullOrWhiteSpace(player.Email));
            });
        }

        [Fact]
        public void GenerateInvalidPlayers_ReturnsInvalidTestData()
        {
            var players = TestDataGenerator.GenerateInvalidPlayers();

            Assert.NotEmpty(players);
            Assert.Contains(players, p => string.IsNullOrWhiteSpace(p.FirstName));
            Assert.Contains(players, p => string.IsNullOrWhiteSpace(p.Email) || !p.Email.Contains("@"));
        }

        [Fact]
        public void GenerateInvalidReservations_ReturnsPastOrInvalidReservations()
        {
            var reservations = TestDataGenerator.GenerateInvalidReservations();

            Assert.NotEmpty(reservations);
            Assert.Contains(reservations, r => r.PlayerId <= 0);
            Assert.Contains(reservations, r => r.ReservedAt < DateTime.Now);
        }
    }

    internal static class TestDataGenerator
    {
        public static List<Player> GenerateRandomPlayers(int count)
        {
            var random = new Random(123);

            var firstNames = new[]
            {
                "Jan", "Anna", "Piotr", "Maria", "Tomasz", "Katarzyna", "Michał", "Ola"
            };

            var lastNames = new[]
            {
                "Kowalski", "Nowak", "Wiśniewski", "Zieliński", "Wójcik", "Kamiński"
            };

            var players = new List<Player>();

            for (int i = 1; i <= count; i++)
            {
                players.Add(new Player
                {
                    FirstName = firstNames[random.Next(firstNames.Length)],
                    LastName = lastNames[random.Next(lastNames.Length)],
                    Email = $"player{i}@test.pl",
                    Phone = $"500{random.Next(100000, 999999)}",
                    CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365))
                });
            }

            return players;
        }

        public static List<Player> GenerateInvalidPlayers()
        {
            return new List<Player>
            {
                new Player
                {
                    FirstName = "",
                    LastName = "BrakImienia",
                    Email = "wrong-email",
                    Phone = "",
                    CreatedAt = DateTime.Now
                },

                new Player
                {
                    FirstName = "Test",
                    LastName = "",
                    Email = "",
                    Phone = "abc",
                    CreatedAt = DateTime.Now
                },

                new Player
                {
                    FirstName = "",
                    LastName = "",
                    Email = "invalid",
                    Phone = "000",
                    CreatedAt = DateTime.Now
                }
            };
        }

        public static List<Reservation> GenerateInvalidReservations()
        {
            return new List<Reservation>
            {
                new Reservation
                {
                    PlayerId = -1,
                    GameSessionId = -1,
                    ReservationStatusId = -1,
                    ReservedAt = DateTime.Now.AddDays(-10)
                },

                new Reservation
                {
                    PlayerId = 0,
                    GameSessionId = 0,
                    ReservationStatusId = 0,
                    ReservedAt = DateTime.Now.AddYears(-1)
                }
            };
        }
    }
}
