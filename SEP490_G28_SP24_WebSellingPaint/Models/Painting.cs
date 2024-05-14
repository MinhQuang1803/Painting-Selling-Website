using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Painting
    {
        public Painting()
        {
            OrderDetails = new HashSet<OrderDetail>();
            Categories = new HashSet<Category>();
            Users = new HashSet<User>();
        }

        public int PaintingId { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public int? Quantity { get; set; }
        public int? ViewCount { get; set; }
        public int? PublishDate { get; set; }
        public int? UserId { get; set; }
        public int? StatusId { get; set; }
        public int? DiscountId { get; set; }
        public bool? IsFragile { get; set; }

        public virtual Discount? Discount { get; set; }
        public virtual Status? Status { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
