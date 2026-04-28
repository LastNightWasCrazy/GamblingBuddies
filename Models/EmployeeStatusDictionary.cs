namespace GamblingBuddies.Models
{
    public class EmployeeStatusDictionary
    {
        public int EmployeeStatusDictionaryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public ICollection<Employee> Employees { get; set; }
    }
}