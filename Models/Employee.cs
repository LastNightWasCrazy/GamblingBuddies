public class Employee
{
    public int EmployeeId { get; set; }

    public int? SystemUserId { get; set; }
    public SystemUser? SystemUser { get; set; }

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    public string? Phone { get; set; }

    public DateTime HireDate { get; set; }

    public int PositionId { get; set; }
    public EmployeePositionDictionary Position { get; set; } = null!;

    public int EmployeeStatusId { get; set; }
    public EmployeeStatusDictionary Status { get; set; } = null!;
}