using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Status
    {
        public Status()
        {
            Accounts = new HashSet<Account>();
            Addresses = new HashSet<Address>();
            Categories = new HashSet<Category>();
            OrderDetails = new HashSet<OrderDetail>();
            Orders = new HashSet<Order>();
            Paintings = new HashSet<Painting>();
            PayAccounts = new HashSet<PayAccount>();
            Posts = new HashSet<Post>();
            Reports = new HashSet<Report>();
            ShippingUnits = new HashSet<ShippingUnit>();
            Vouchers = new HashSet<Voucher>();
        }

        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? TypeId { get; set; }

        public virtual Type? Type { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Painting> Paintings { get; set; }
        public virtual ICollection<PayAccount> PayAccounts { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<ShippingUnit> ShippingUnits { get; set; }
        public virtual ICollection<Voucher> Vouchers { get; set; }
    }
}
