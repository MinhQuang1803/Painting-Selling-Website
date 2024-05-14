using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Type
    {
        public Type()
        {
            Addresses = new HashSet<Address>();
            Categories = new HashSet<Category>();
            Comments = new HashSet<Comment>();
            Images = new HashSet<Image>();
            Reports = new HashSet<Report>();
            ShippingPrices = new HashSet<ShippingPrice>();
            Statuses = new HashSet<Status>();
            TransactionHistories = new HashSet<TransactionHistory>();
        }

        public int TypeId { get; set; }
        public string? TypeName { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<ShippingPrice> ShippingPrices { get; set; }
        public virtual ICollection<Status> Statuses { get; set; }
        public virtual ICollection<TransactionHistory> TransactionHistories { get; set; }
    }
}
