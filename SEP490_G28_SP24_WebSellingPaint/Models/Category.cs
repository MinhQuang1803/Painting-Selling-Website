using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Category
    {
        public Category()
        {
            Paintings = new HashSet<Painting>();
            Posts = new HashSet<Post>();
            Reports = new HashSet<Report>();
        }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public int? TypeId { get; set; }
        public int? StatusId { get; set; }

        public virtual Status? Status { get; set; }
        public virtual Type? Type { get; set; }

        public virtual ICollection<Painting> Paintings { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
    }
}
