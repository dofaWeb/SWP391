using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SWP391_FinalProject.Entities;

public partial class Swp391Se1801Group3Context : DbContext
{
    public Swp391Se1801Group3Context()
    {
    }

    public Swp391Se1801Group3Context(DbContextOptions<Swp391Se1801Group3Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<ChangeReason> ChangeReasons { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<DiscountLog> DiscountLogs { get; set; }

    public virtual DbSet<NameLog> NameLogs { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderState> OrderStates { get; set; }

    public virtual DbSet<PriceLog> PriceLogs { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductConfiguration> ProductConfigurations { get; set; }

    public virtual DbSet<ProductItem> ProductItems { get; set; }

    public virtual DbSet<ProductLog> ProductLogs { get; set; }

    public virtual DbSet<ProductState> ProductStates { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<QuantityLog> QuantityLogs { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<RoleName> RoleNames { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<StaffShift> StaffShifts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAddress> UserAddresses { get; set; }

    public virtual DbSet<Variation> Variations { get; set; }

    public virtual DbSet<VariationOption> VariationOptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(local);Database=SWP391_SE1801_GROUP3;UID=sa;PWD=123456;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3213E83F081D0FEA");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Username, "UQ__Account__F3DBC57242CA8829").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Password)
                .HasMaxLength(32)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId)
                .HasMaxLength(8)
                .HasColumnName("role_id");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Role_Name");
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Address__3213E83F95349393");

            entity.ToTable("Address");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Address1)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.ProvinceId)
                .HasMaxLength(8)
                .HasColumnName("province_id");

            entity.HasOne(d => d.Province).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.ProvinceId)
                .HasConstraintName("FK_Address_Province");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Brand__3213E83F7B8F9ED0");

            entity.ToTable("Brand");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ChangeReason>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Change_R__3213E83F1C5B6DD6");

            entity.ToTable("Change_Reason");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Reason)
                .HasMaxLength(50)
                .HasColumnName("reason");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comment__3213E83FAC0F8B08");

            entity.ToTable("Comment");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Comment1)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .HasColumnName("user_id");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comment_Product_Item");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comment_User");
        });

        modelBuilder.Entity<DiscountLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Discount__3213E83FDBD0DA00");

            entity.ToTable("Discount_Log");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.ChangeTimestamp)
                .HasColumnType("datetime")
                .HasColumnName("change_timestamp");
            entity.Property(e => e.NewDiscount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("new_discount");
            entity.Property(e => e.OldDiscount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("old_discount");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.DiscountLogs)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Discount_Log_Product_Item");
        });

        modelBuilder.Entity<NameLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Name_Log__3213E83FD47661A7");

            entity.ToTable("Name_Log");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.ChangeTimestamp)
                .HasColumnType("datetime")
                .HasColumnName("change_timestamp");
            entity.Property(e => e.NewName)
                .HasMaxLength(50)
                .HasColumnName("new_name");
            entity.Property(e => e.OldName)
                .HasMaxLength(50)
                .HasColumnName("old_name");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.NameLogs)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Name_Log_Product_Item");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order__3213E83F391F7BAA");

            entity.ToTable("Order");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.EarnPoint)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("earn_point");
            entity.Property(e => e.StaffShiftId)
                .HasMaxLength(8)
                .HasColumnName("staff_shift_id");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.UsePoint)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("use_point");
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .HasColumnName("user_id");

            entity.HasOne(d => d.StaffShift).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StaffShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Staff_Shift");

            entity.HasOne(d => d.State).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_State");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_User");
        });

        modelBuilder.Entity<OrderState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order_St__3213E83FBF296486");

            entity.ToTable("Order_State");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PriceLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Price_Lo__3213E83FB9382986");

            entity.ToTable("Price_Log");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.ChangeTimestamp)
                .HasColumnType("datetime")
                .HasColumnName("change_timestamp");
            entity.Property(e => e.NewPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("new_price");
            entity.Property(e => e.OldPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("old_price");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.PriceLogs)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Price_Log_Product_Item");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3213E83F3168839E");

            entity.ToTable("Product");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.BrandId)
                .HasMaxLength(8)
                .HasColumnName("brand_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Picture)
                .HasMaxLength(255)
                .HasColumnName("picture");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Brand");

            entity.HasOne(d => d.State).WithMany(p => p.Products)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_State");
        });

        modelBuilder.Entity<ProductConfiguration>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Product_Configuration");

            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id");
            entity.Property(e => e.VariationOptionId)
                .HasMaxLength(8)
                .HasColumnName("variation_option_id");

            entity.HasOne(d => d.ProductItem).WithMany()
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Configuration_Product_Item");

            entity.HasOne(d => d.VariationOption).WithMany()
                .HasForeignKey(d => d.VariationOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Configuration_Variation_Option");
        });

        modelBuilder.Entity<ProductItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product___3213E83F128AB5AB");

            entity.ToTable("Product_Item");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Discount)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("discount");
            entity.Property(e => e.ImportPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("import_price");
            entity.Property(e => e.ProductId)
                .HasMaxLength(8)
                .HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SellingPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("selling_price");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Item_Product");
        });

        modelBuilder.Entity<ProductLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product___3213E83FE0ABB2AE");

            entity.ToTable("Product_Log");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.ChangeReasonId)
                .HasMaxLength(8)
                .HasColumnName("change_reason_id");
            entity.Property(e => e.DisocuntLogId)
                .HasMaxLength(8)
                .HasColumnName("disocunt_log_id");
            entity.Property(e => e.NameLogId)
                .HasMaxLength(8)
                .HasColumnName("name_log_id");
            entity.Property(e => e.PriceLogId)
                .HasMaxLength(8)
                .HasColumnName("price_log_id");
            entity.Property(e => e.QuantityLogId)
                .HasMaxLength(8)
                .HasColumnName("quantity_log_id");

            entity.HasOne(d => d.ChangeReason).WithMany(p => p.ProductLogs)
                .HasForeignKey(d => d.ChangeReasonId)
                .HasConstraintName("FK_Product_Log_Change_Reason");

            entity.HasOne(d => d.DisocuntLog).WithMany(p => p.ProductLogs)
                .HasForeignKey(d => d.DisocuntLogId)
                .HasConstraintName("FK_Product_Log_Discount_Log");

            entity.HasOne(d => d.NameLog).WithMany(p => p.ProductLogs)
                .HasForeignKey(d => d.NameLogId)
                .HasConstraintName("FK_Product_Log_Name_Log");

            entity.HasOne(d => d.PriceLog).WithMany(p => p.ProductLogs)
                .HasForeignKey(d => d.PriceLogId)
                .HasConstraintName("FK_Product_Log_Price_Log");

            entity.HasOne(d => d.QuantityLog).WithMany(p => p.ProductLogs)
                .HasForeignKey(d => d.QuantityLogId)
                .HasConstraintName("FK_Product_Log_Quantity_Log");
        });

        modelBuilder.Entity<ProductState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product___3213E83FD354BF97");

            entity.ToTable("Product_State");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Province__3213E83F65CE2506");

            entity.ToTable("Province");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<QuantityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Quantity__3213E83FEE1C1205");

            entity.ToTable("Quantity_Log");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.ChangeTimestamp)
                .HasColumnType("datetime")
                .HasColumnName("change_timestamp");
            entity.Property(e => e.NewQuantity).HasColumnName("new_quantity");
            entity.Property(e => e.OldQuantity).HasColumnName("old_quantity");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.QuantityLogs)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Quantity_Log_Product_Item");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rating__3213E83F2C1FB06E");

            entity.ToTable("Rating");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id");
            entity.Property(e => e.Rating1).HasColumnName("rating");
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .HasColumnName("user_id");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rating_Product_Item");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rating_User");
        });

        modelBuilder.Entity<RoleName>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role_Nam__3213E83F972B57C4");

            entity.ToTable("Role_Name");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Staff__46A222CD7E0759EB");

            entity.Property(e => e.AccountId)
                .HasMaxLength(8)
                .HasColumnName("account_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Salary).HasColumnName("salary");

            entity.HasOne(d => d.Account).WithOne(p => p.Staff)
                .HasForeignKey<Staff>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Staff_Account");
        });

        modelBuilder.Entity<StaffShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Staff_Sh__3213E83FD7C8233B");

            entity.ToTable("Staff_Shift");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.ShiftTimeBegin)
                .HasColumnType("datetime")
                .HasColumnName("shift_time_begin");
            entity.Property(e => e.ShiftTimeEnd)
                .HasColumnType("datetime")
                .HasColumnName("shift_time_end");
            entity.Property(e => e.StaffId)
                .HasMaxLength(8)
                .HasColumnName("staff_id");

            entity.HasOne(d => d.Staff).WithMany(p => p.StaffShifts)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Staff_Shift_Staff");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__User__46A222CDE0177B65");

            entity.ToTable("User");

            entity.Property(e => e.AccountId)
                .HasMaxLength(8)
                .HasColumnName("account_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Point).HasColumnName("point");

            entity.HasOne(d => d.Account).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Account");
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_Add__B9BE370FFB9A75F0");

            entity.ToTable("User_Address");

            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .HasColumnName("user_id");
            entity.Property(e => e.AddressId)
                .HasMaxLength(8)
                .HasColumnName("address_id");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");

            entity.HasOne(d => d.Address).WithMany(p => p.UserAddresses)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_User_Address_Address");

            entity.HasOne(d => d.User).WithOne(p => p.UserAddress)
                .HasForeignKey<UserAddress>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Address_User");
        });

        modelBuilder.Entity<Variation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Variatio__3213E83F9C2DC31D");

            entity.ToTable("Variation");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<VariationOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Variatio__3213E83F6CA3C625");

            entity.ToTable("Variation_Option");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id");
            entity.Property(e => e.Value)
                .HasMaxLength(50)
                .HasColumnName("value");
            entity.Property(e => e.VariationId)
                .HasMaxLength(8)
                .HasColumnName("variation_id");

            entity.HasOne(d => d.Variation).WithMany(p => p.VariationOptions)
                .HasForeignKey(d => d.VariationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Variation_Option_Variation");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
