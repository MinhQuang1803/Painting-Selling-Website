using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class ShippingPrice
    {
        public int PriceId { get; set; }
        public int? ShippingUnitId { get; set; }
        public decimal? Price { get; set; }
        public int? TypeId { get; set; }
        public int? PerKm { get; set; }
        public int? StartDate { get; set; }

        public virtual ShippingUnit? ShippingUnit { get; set; }
        public virtual Type? Type { get; set; }
    }
}
