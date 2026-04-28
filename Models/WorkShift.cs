namespace GamblingBuddies.Models
{
    public class WorkShift
    {
        public int WorkShiftId { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public int CreatedByUserId { get; set; }
        public SystemUser CreatedByUser { get; set; }
    }
}
