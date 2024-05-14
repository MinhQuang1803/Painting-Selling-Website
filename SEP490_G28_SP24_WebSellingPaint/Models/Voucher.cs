using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Voucher
    {
        public Voucher()
        {
            OrderVouchers = new HashSet<OrderVoucher>();
        }

        public int VoucherId { get; set; }
        public string? VoucherName { get; set; }
        public string? VoucherCode { get; set; }
        public int? StartDate { get; set; }
        public int? EndDate { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? MinOrderValue { get; set; }
        public int? UserId { get; set; }
        public int? StatusId { get; set; }

        public virtual Status? Status { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<OrderVoucher> OrderVouchers { get; set; }
    }
}
