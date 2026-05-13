public class EmployeeAssignment
{
    public int EmployeeAssignmentId { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }

    public int GameSessionId { get; set; }
    public GameSession GameSession { get; set; }

    public string? Notes { get; set; }

    public int AssignedByUserId { get; set; }
    public SystemUser AssignedByUser { get; set; }
}