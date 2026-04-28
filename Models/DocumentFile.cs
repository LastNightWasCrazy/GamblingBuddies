using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class DocumentFile
    {
        [Key]
        public int DocumentFileId { get; set; }

        public int DocumentId { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }

        public Document Document { get; set; }
    }
}
