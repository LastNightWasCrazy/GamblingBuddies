using GamblingBuddies.TestData;

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
}