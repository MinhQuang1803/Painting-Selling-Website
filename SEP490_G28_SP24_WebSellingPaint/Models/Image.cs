using System;
using System.Collections.Generic;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class Image
    {
        public int ImageId { get; set; }
        public string? ImageUrl { get; set; }
        public int? ObjectId { get; set; }
        public int? TypeId { get; set; }

        public virtual Type? Type { get; set; }
    }
}
