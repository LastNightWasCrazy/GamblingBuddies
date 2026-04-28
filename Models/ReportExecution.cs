namespace GamblingBuddies.Models
{
    public class ReportExecution
    {
        public int ReportExecutionId { get; set; }
        public int ReportDefinitionId { get; set; }
        public ReportDefinition ReportDefinition { get; set; }
        public int GeneratedByUserId { get; set; }
        public SystemUser GeneratedByUser { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string ParametersJson { get; set; }
    }
}
