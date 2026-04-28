namespace GamblingBuddies.Models
{
    public class EmployeePositionDictionary
    {
        public int EmployeePositionDictionaryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public ICollection<Employee> Employees { get; set; }
    }
}