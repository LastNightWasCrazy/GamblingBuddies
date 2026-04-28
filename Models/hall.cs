namespace GamblingBuddies.Models
{
    public class Hall
    {
        public int HallId { get; set; }

        public string Name { get; set; }

        public int HallTypeId { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public ICollection<GameTable> GameTables { get; set; } = new List<GameTable>();
    }
}
