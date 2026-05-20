using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamblingBuddies.Models
{
    public class PaymentReport
    {
        [Key]
        public int PaymentReportId { get; set; }

        [Required]
        public string ReportName { get; set; }

        public string Description { get; set; }

        public DateTime GeneratedDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal TotalAmount { get; set; }

        public int TransactionsCount { get; set; }

        public string FiltersApplied { get; set; }

        public int GeneratedByUserId { get; set; }

        [ForeignKey("GeneratedByUserId")]
        public SystemUser GeneratedByUser { get; set; }

        public string? PdfFilePath { get; set; }
    }
}