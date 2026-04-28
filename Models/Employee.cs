using System;

namespace GamblingBuddies.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public int? SystemUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public DateTime HireDate { get; set; }
        public int PositionId { get; set; }
        public int EmployeeStatusId { get; set; }
        public int EmployeePositionDictionaryId { get; set; }
        public int EmployeeStatusDictionaryId { get; set; }

        public EmployeePositionDictionary Position { get; set; }
        public EmployeeStatusDictionary Status { get; set; }
        public SystemUser SystemUser { get; set; }
    }
}