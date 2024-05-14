using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class TransactionHistory
    {
        public TransactionHistory()
        {
            UserTransactions = new HashSet<UserTransaction>();
        }

        public int TransactionId { get; set; }
        public string? PaymentId { get; set; }
        public int? Time { get; set; }
        public string? Content { get; set; }
        public decimal? Amount { get; set; }
        public int? StatusId { get; set; }
        public int? TypeId { get; set; }

        public virtual Type? Type { get; set; }
        public virtual ICollection<UserTransaction> UserTransactions { get; set; }
    }
}
