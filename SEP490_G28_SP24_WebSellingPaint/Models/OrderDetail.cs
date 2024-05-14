using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int? Quantity { get; set; }
        public int? StatusId { get; set; }
        public int? ReadyDate { get; set; }
        public int? Discount { get; set; }
        public int? PaintingId { get; set; }
        public int? OrderId { get; set; }
        public decimal? Price { get; set; }

        public virtual Order? Order { get; set; }
        public virtual Painting? Painting { get; set; }
        public virtual Status? Status { get; set; }
    }
}
