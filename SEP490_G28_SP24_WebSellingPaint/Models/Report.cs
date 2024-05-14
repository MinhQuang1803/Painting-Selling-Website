using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Report
    {
        public Report()
        {
            Categories = new HashSet<Category>();
        }

        public int ReportId { get; set; }
        public int? ReportDate { get; set; }
        public string? ReportNote { get; set; }
        public int? TypeId { get; set; }
        public int? ObjectId { get; set; }
        public int? StatusId { get; set; }
        public int? UserId { get; set; }
        public int? SupervisorId { get; set; }

        public virtual Status? Status { get; set; }
        public virtual User? Supervisor { get; set; }
        public virtual Type? Type { get; set; }
        public virtual User? User { get; set; }

        public virtual ICollection<Category> Categories { get; set; }
    }
}
