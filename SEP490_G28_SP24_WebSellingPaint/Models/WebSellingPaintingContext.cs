using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SEP490_G28_SP24_WebSellingPaint.Models
{
    public partial class WebSellingPaintingContext : DbContext
    {
        public WebSellingPaintingContext()
        {
        }

        public WebSellingPaintingContext(DbContextOptions<WebSellingPaintingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<Address> Addresses { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<Discount> Discounts { get; set; } = null!;
        public virtual DbSet<Image> Images { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public virtual DbSet<OrderVoucher> OrderVouchers { get; set; } = null!;
        public virtual DbSet<Painting> Paintings { get; set; } = null!;
        public virtual DbSet<PayAccount> PayAccounts { get; set; } = null!;
        public virtual DbSet<Post> Posts { get; set; } = null!;
        public virtual DbSet<Report> Reports { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<ShippingPrice> ShippingPrices { get; set; } = null!;
        public virtual DbSet<ShippingUnit> ShippingUnits { get; set; } = null!;
        public virtual DbSet<Status> Statuses { get; set; } = null!;
        public virtual DbSet<TransactionHistory> TransactionHistories { get; set; } = null!;
        public virtual DbSet<Type> Types { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserTransaction> UserTransactions { get; set; } = null!;
        public virtual DbSet<Voucher> Vouchers { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.

                optionsBuilder.UseSqlServer("server=14.225.205.28,1437;database=WebSellingPainting;uid=sa;pwd=Abc@123123;Trusted_Connection=True;Integrated Security=False;");

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK__Account__RoleID__4AB81AF0");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__Account__StatusI__4BAC3F29");
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");

                entity.Property(e => e.AddressId).HasColumnName("AddressID");

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.District).HasMaxLength(50);

                entity.Property(e => e.ObjectId).HasColumnName("ObjectID");

                entity.Property(e => e.Street).HasMaxLength(50);

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.HasOne(d => d.StatusNavigation)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.Status)
                    .HasConstraintName("FK__Address__Status__403A8C7D");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__Address__TypeID__412EB0B6");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.CategoryName).HasMaxLength(50);

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Categories)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__Category__Status__47DBAE45");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Categories)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__Category__TypeID__46E78A0C");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comment");

                entity.Property(e => e.CommentId).HasColumnName("CommentID");

                entity.Property(e => e.CommentDate).HasColumnType("datetime");

                entity.Property(e => e.CommentRepId).HasColumnName("CommentRepID");

                entity.Property(e => e.ObjectId).HasColumnName("ObjectID");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.CommentRep)
                    .WithMany(p => p.InverseCommentRep)
                    .HasForeignKey(d => d.CommentRepId)
                    .HasConstraintName("FK__Comment__Comment__571DF1D5");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__Comment__TypeID__5535A963");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Comment__UserID__5629CD9C");
            });

            modelBuilder.Entity<Discount>(entity =>
            {
                entity.ToTable("Discount");

                entity.Property(e => e.DiscountId).HasColumnName("DiscountID");
            });

            modelBuilder.Entity<Image>(entity =>
            {
                entity.ToTable("Image");

                entity.Property(e => e.ImageId).HasColumnName("ImageID");

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.ObjectId).HasColumnName("ObjectID");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Images)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__Image__TypeID__440B1D61");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.FromAddressId).HasColumnName("FromAddressID");

                entity.Property(e => e.OrderCode).HasMaxLength(30);

                entity.Property(e => e.PaymentMethod).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.ReceiverName).HasMaxLength(50);

                entity.Property(e => e.ShippingUnitId).HasColumnName("ShippingUnitID");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.ToAddressId).HasColumnName("ToAddressID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.FromAddress)
                    .WithMany(p => p.OrderFromAddresses)
                    .HasForeignKey(d => d.FromAddressId)
                    .HasConstraintName("FK__Order__FromAddre__6FE99F9F");

                entity.HasOne(d => d.ShippingUnit)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.ShippingUnitId)
                    .HasConstraintName("FK__Order__ShippingU__6E01572D");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__Order__StatusID__6EF57B66");

                entity.HasOne(d => d.ToAddress)
                    .WithMany(p => p.OrderToAddresses)
                    .HasForeignKey(d => d.ToAddressId)
                    .HasConstraintName("FK__Order__ToAddress__70DDC3D8");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Order__UserID__6D0D32F4");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.PaintingId).HasColumnName("PaintingID");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK__OrderDeta__Order__75A278F5");

                entity.HasOne(d => d.Painting)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.PaintingId)
                    .HasConstraintName("FK__OrderDeta__Paint__74AE54BC");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__OrderDeta__Statu__73BA3083");
            });

            modelBuilder.Entity<OrderVoucher>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.OrderId, e.VoucherId })
                    .HasName("PK__OrderVou__3B8B276F1C433AF3");

                entity.ToTable("OrderVoucher");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.OrderId).HasColumnName("OrderID");

                entity.Property(e => e.VoucherId).HasColumnName("VoucherID");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderVouchers)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__OrderVouc__Order__160F4887");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.OrderVouchers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__OrderVouc__UserI__151B244E");

                entity.HasOne(d => d.Voucher)
                    .WithMany(p => p.OrderVouchers)
                    .HasForeignKey(d => d.VoucherId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__OrderVouc__Vouch__17036CC0");
            });

            modelBuilder.Entity<Painting>(entity =>
            {
                entity.ToTable("Painting");

                entity.Property(e => e.PaintingId).HasColumnName("PaintingID");

                entity.Property(e => e.DiscountId).HasColumnName("DiscountID");

                entity.Property(e => e.Height).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.Width).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.Paintings)
                    .HasForeignKey(d => d.DiscountId)
                    .HasConstraintName("FK__Painting__Discou__5BE2A6F2");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Paintings)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__Painting__Status__5AEE82B9");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PaintingsNavigation)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Painting__UserID__59FA5E80");

                entity.HasMany(d => d.Categories)
                    .WithMany(p => p.Paintings)
                    .UsingEntity<Dictionary<string, object>>(
                        "PaintingCategory",
                        l => l.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__PaintingC__Categ__02FC7413"),
                        r => r.HasOne<Painting>().WithMany().HasForeignKey("PaintingId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__PaintingC__Paint__02084FDA"),
                        j =>
                        {
                            j.HasKey("PaintingId", "CategoryId").HasName("PK__Painting__6EBD04B0C1B6F2F4");

                            j.ToTable("PaintingCategory");

                            j.IndexerProperty<int>("PaintingId").HasColumnName("PaintingID");

                            j.IndexerProperty<int>("CategoryId").HasColumnName("CategoryID");
                        });
            });

            modelBuilder.Entity<PayAccount>(entity =>
            {
                entity.ToTable("PayAccount");

                entity.Property(e => e.ActiveFee).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.PayAccounts)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__PayAccoun__Accou__0C85DE4D");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.PayAccounts)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__PayAccoun__Statu__0D7A0286");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("Post");

                entity.Property(e => e.PostId).HasColumnName("PostID");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__Post__StatusID__628FA481");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Post__UserID__619B8048");

                entity.HasMany(d => d.Categories)
                    .WithMany(p => p.Posts)
                    .UsingEntity<Dictionary<string, object>>(
                        "PostCategory",
                        l => l.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__PostCateg__Categ__66603565"),
                        r => r.HasOne<Post>().WithMany().HasForeignKey("PostId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__PostCateg__PostI__656C112C"),
                        j =>
                        {
                            j.HasKey("PostId", "CategoryId").HasName("PK__PostCate__0B82F39A03FC7F68");

                            j.ToTable("PostCategory");

                            j.IndexerProperty<int>("PostId").HasColumnName("PostID");

                            j.IndexerProperty<int>("CategoryId").HasColumnName("CategoryID");
                        });
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("Report");

                entity.Property(e => e.ReportId).HasColumnName("ReportID");

                entity.Property(e => e.ObjectId).HasColumnName("ObjectID");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.SupervisorId).HasColumnName("SupervisorID");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__Report__StatusID__797309D9");

                entity.HasOne(d => d.Supervisor)
                    .WithMany(p => p.ReportSupervisors)
                    .HasForeignKey(d => d.SupervisorId)
                    .HasConstraintName("FK__Report__Supervis__7B5B524B");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__Report__TypeID__787EE5A0");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ReportUsers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Report__UserID__7A672E12");

                entity.HasMany(d => d.Categories)
                    .WithMany(p => p.Reports)
                    .UsingEntity<Dictionary<string, object>>(
                        "ReportCategory",
                        l => l.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ReportCat__Categ__7F2BE32F"),
                        r => r.HasOne<Report>().WithMany().HasForeignKey("ReportId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ReportCat__Repor__7E37BEF6"),
                        j =>
                        {
                            j.HasKey("ReportId", "CategoryId").HasName("PK__ReportCa__742DDB4711E50F97");

                            j.ToTable("ReportCategory");

                            j.IndexerProperty<int>("ReportId").HasColumnName("ReportID");

                            j.IndexerProperty<int>("CategoryId").HasColumnName("CategoryID");
                        });
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.RoleName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ShippingPrice>(entity =>
            {
                entity.HasKey(e => e.PriceId)
                    .HasName("PK__Shipping__4957584FC37CBFCB");

                entity.ToTable("ShippingPrice");

                entity.Property(e => e.PriceId).HasColumnName("PriceID");

                entity.Property(e => e.PerKm).HasColumnName("PerKM");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.ShippingUnitId).HasColumnName("ShippingUnitID");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.HasOne(d => d.ShippingUnit)
                    .WithMany(p => p.ShippingPrices)
                    .HasForeignKey(d => d.ShippingUnitId)
                    .HasConstraintName("FK__ShippingP__Shipp__693CA210");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.ShippingPrices)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__ShippingP__TypeI__6A30C649");
            });

            modelBuilder.Entity<ShippingUnit>(entity =>
            {
                entity.ToTable("ShippingUnit");

                entity.Property(e => e.ShippingUnitId).HasColumnName("ShippingUnitID");

                entity.Property(e => e.Email)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.Website).IsUnicode(false);

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.ShippingUnits)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__ShippingU__Statu__5EBF139D");
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("Status");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.StatusName).HasMaxLength(50);

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Statuses)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__Status__TypeID__3B75D760");
            });

            modelBuilder.Entity<TransactionHistory>(entity =>
            {
                entity.HasKey(e => e.TransactionId)
                    .HasName("PK__Transact__55433A4B969AF37B");

                entity.ToTable("TransactionHistory");

                entity.Property(e => e.TransactionId).HasColumnName("TransactionID");

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.PaymentId).HasColumnName("PaymentID");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.TransactionHistories)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__Transacti__TypeI__09A971A2");
            });

            modelBuilder.Entity<Type>(entity =>
            {
                entity.ToTable("Type");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.Property(e => e.TypeName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.AccountId).HasColumnName("AccountID");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__User__AccountID__4E88ABD4");

                entity.HasMany(d => d.Paintings)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "Cart",
                        l => l.HasOne<Painting>().WithMany().HasForeignKey("PaintingId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__Cart__PaintingID__06CD04F7"),
                        r => r.HasOne<User>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__Cart__UserID__05D8E0BE"),
                        j =>
                        {
                            j.HasKey("UserId", "PaintingId").HasName("PK__Cart__2B7A15DD924CBAC2");

                            j.ToTable("Cart");

                            j.IndexerProperty<int>("UserId").HasColumnName("UserID");

                            j.IndexerProperty<int>("PaintingId").HasColumnName("PaintingID");
                        });
            });

            modelBuilder.Entity<UserTransaction>(entity =>
            {
                entity.ToTable("UserTransaction");

                entity.HasOne(d => d.PayAccount)
                    .WithMany(p => p.UserTransactions)
                    .HasForeignKey(d => d.PayAccountId)
                    .HasConstraintName("FK__UserTrans__PayAc__114A936A");

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.UserTransactions)
                    .HasForeignKey(d => d.TransactionId)
                    .HasConstraintName("FK__UserTrans__Trans__10566F31");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserTransactions)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__UserTrans__UserI__123EB7A3");
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.ToTable("Voucher");

                entity.Property(e => e.VoucherId).HasColumnName("VoucherID");

                entity.Property(e => e.MinOrderValue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Percentage).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.StatusId).HasColumnName("StatusID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.VoucherCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VoucherName).HasMaxLength(50);

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Vouchers)
                    .HasForeignKey(d => d.StatusId)
                    .HasConstraintName("FK__Voucher__StatusI__52593CB8");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Vouchers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Voucher__UserID__5165187F");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
