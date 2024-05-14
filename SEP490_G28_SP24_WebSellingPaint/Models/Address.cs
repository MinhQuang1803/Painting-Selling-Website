using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Address
    {
        public Address()
        {
            OrderFromAddresses = new HashSet<Order>();
            OrderToAddresses = new HashSet<Order>();
        }

        public int AddressId { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Street { get; set; }
        public string? Detail { get; set; }
        public int? Status { get; set; }
        public int? ObjectId { get; set; }
        public int? TypeId { get; set; }

        public virtual Status? StatusNavigation { get; set; }
        public virtual Type? Type { get; set; }
        public virtual ICollection<Order> OrderFromAddresses { get; set; }
        public virtual ICollection<Order> OrderToAddresses { get; set; }
    }
}
