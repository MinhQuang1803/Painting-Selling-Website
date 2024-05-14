using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Discount
    {
        public Discount()
        {
            Paintings = new HashSet<Painting>();
        }

        public int DiscountId { get; set; }
        public int? Percentage { get; set; }
        public int? StartDate { get; set; }
        public int? EndDate { get; set; }

        public virtual ICollection<Painting> Paintings { get; set; }
    }
}
