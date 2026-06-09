using GamblingBuddies.Models;

namespace GamblingBuddies.TestData
{
    public static class TestDataGenerator
    {
        public static List<Player> GenerateRandomPlayers(int count)
        {
            var random = new Random();

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