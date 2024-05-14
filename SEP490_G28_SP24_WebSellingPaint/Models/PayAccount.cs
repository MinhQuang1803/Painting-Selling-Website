using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class PayAccount
    {
        public PayAccount()
        {
            UserTransactions = new HashSet<UserTransaction>();
        }

        public int PayAccountId { get; set; }
        public int? AccountId { get; set; }
        public int? ActiveAccountDate { get; set; }
        public int? ExpiredAccountDate { get; set; }
        public string? ActiveCode { get; set; }
        public decimal? ActiveFee { get; set; }
        public int? StatusId { get; set; }

        public virtual Account? Account { get; set; }
        public virtual Status? Status { get; set; }
        public virtual ICollection<UserTransaction> UserTransactions { get; set; }
    }
}
