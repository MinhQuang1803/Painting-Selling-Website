using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Post
    {
        public Post()
        {
            Categories = new HashSet<Category>();
        }

        public int PostId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? Date { get; set; }
        public int? ViewCount { get; set; }
        public int? UserId { get; set; }
        public int? StatusId { get; set; }

        public virtual Status? Status { get; set; }
        public virtual User? User { get; set; }

        public virtual ICollection<Category> Categories { get; set; }
    }
}
