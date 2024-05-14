using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class OrderVoucher
    {
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public int VoucherId { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual Voucher Voucher { get; set; } = null!;
    }
}
