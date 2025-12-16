using System;

namespace BADEAPORTAL.Models
{
    public class MemoPdfRequest
    {
        public string To { get; set; } = null!;
        public string? Through { get; set; }
        public string From { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Classification { get; set; } = null!;
        public string BodyHtml { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }
        public string CreatedByName { get; set; } = null!;
    }
}
