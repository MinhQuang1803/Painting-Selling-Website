using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class ShippingUnit
    {
        public ShippingUnit()
        {
            Orders = new HashSet<Order>();
            ShippingPrices = new HashSet<ShippingPrice>();
        }

        public int ShippingUnitId { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public int? StatusId { get; set; }

        public virtual Status? Status { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ShippingPrice> ShippingPrices { get; set; }
    }
}
