using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class User
    {
        public User()
        {
            Comments = new HashSet<Comment>();
            OrderVouchers = new HashSet<OrderVoucher>();
            Orders = new HashSet<Order>();
            PaintingsNavigation = new HashSet<Painting>();
            Posts = new HashSet<Post>();
            ReportSupervisors = new HashSet<Report>();
            ReportUsers = new HashSet<Report>();
            UserTransactions = new HashSet<UserTransaction>();
            Vouchers = new HashSet<Voucher>();
            Paintings = new HashSet<Painting>();
        }

        public int UserId { get; set; }
        public string? UserName { get; set; }
        public bool? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ShopName { get; set; }
        public string? ArtistBackground { get; set; }
        public int? AccountId { get; set; }

        public virtual Account? Account { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<OrderVoucher> OrderVouchers { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Painting> PaintingsNavigation { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Report> ReportSupervisors { get; set; }
        public virtual ICollection<Report> ReportUsers { get; set; }
        public virtual ICollection<UserTransaction> UserTransactions { get; set; }
        public virtual ICollection<Voucher> Vouchers { get; set; }

        public virtual ICollection<Painting> Paintings { get; set; }
    }
}
