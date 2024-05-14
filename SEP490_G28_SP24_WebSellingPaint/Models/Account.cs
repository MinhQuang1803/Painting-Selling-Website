using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Account
    {
        public Account()
        {
            PayAccounts = new HashSet<PayAccount>();
            Users = new HashSet<User>();
        }

        public int AccountId { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? RoleId { get; set; }
        public int? StatusId { get; set; }

        public virtual Role? Role { get; set; }
        public virtual Status? Status { get; set; }
        public virtual ICollection<PayAccount> PayAccounts { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
