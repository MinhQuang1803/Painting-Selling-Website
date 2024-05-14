using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            OrderVouchers = new HashSet<OrderVoucher>();
        }

        public int OrderId { get; set; }
        public string? OrderNote { get; set; }
        public int? OrderDate { get; set; }
        public int? ShipDate { get; set; }
        public string? ReceiverName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PaymentMethod { get; set; }
        public int? UserId { get; set; }
        public int? ShippingUnitId { get; set; }
        public int? StatusId { get; set; }
        public int? FromAddressId { get; set; }
        public int? ToAddressId { get; set; }
        public string? OrderCode { get; set; }

        public virtual Address? FromAddress { get; set; }
        public virtual ShippingUnit? ShippingUnit { get; set; }
        public virtual Status? Status { get; set; }
        public virtual Address? ToAddress { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<OrderVoucher> OrderVouchers { get; set; }
    }
}
