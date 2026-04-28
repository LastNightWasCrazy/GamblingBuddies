namespace GamblingBuddies.Models
{
    public class ReportDefinition
    {
        public int ReportDefinitionId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string QueryTemplate_or_Definition { get; set; }
        public int CreatedByUserId { get; set; }
        public SystemUser CreatedByUser { get; set; }


    }
}
