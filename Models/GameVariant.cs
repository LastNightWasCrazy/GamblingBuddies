namespace GamblingBuddies.Models
{
    public class GameVariant
    {
        public int GameVariantId { get; set; }

        public int GameId { get; set; }

        public string Name { get; set; }

        public string RulesDescription { get; set; }

        public decimal DefaultMinBet { get; set; }
        public decimal DefaultMaxBet { get; set; }

        public bool IsActive { get; set; }

        public Game Game { get; set; }
    }
}