using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class UserTransaction
    {
        public int UserTransactionId { get; set; }
        public int? TransactionId { get; set; }
        public int? PayAccountId { get; set; }
        public int? UserId { get; set; }

        public virtual PayAccount? PayAccount { get; set; }
        public virtual TransactionHistory? Transaction { get; set; }
        public virtual User? User { get; set; }
    }
}
