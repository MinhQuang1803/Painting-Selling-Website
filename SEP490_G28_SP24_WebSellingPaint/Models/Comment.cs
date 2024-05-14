using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Comment
    {
        public Comment()
        {
            InverseCommentRep = new HashSet<Comment>();
        }

        public int CommentId { get; set; }
        public string? Content { get; set; }
        public DateTime? CommentDate { get; set; }
        public int? ObjectId { get; set; }
        public int? TypeId { get; set; }
        public int? UserId { get; set; }
        public int? CommentRepId { get; set; }

        public virtual Comment? CommentRep { get; set; }
        public virtual Type? Type { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<Comment> InverseCommentRep { get; set; }
    }
}
