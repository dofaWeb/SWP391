using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace SWP391_FinalProject.Entities;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ChangeReason> ChangeReasons { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<DiscountLog> DiscountLogs { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderState> OrderStates { get; set; }

    public virtual DbSet<PriceLog> PriceLogs { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductConfiguration> ProductConfigurations { get; set; }

    public virtual DbSet<ProductItem> ProductItems { get; set; }

    public virtual DbSet<ProductLog> ProductLogs { get; set; }

    public virtual DbSet<ProductNameLog> ProductNameLogs { get; set; }

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
        => optionsBuilder.UseMySql("server=mysql-35c69a44-swp391-group3.i.aivencloud.com;port=25832;database=SWP391;user=avnadmin;password=AVNS_mzCmZ_1hz1gM4yr03o8;sslmode=Required", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.30-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Account");

            entity.HasIndex(e => e.RoleId, "FK_Account_Role_Name");

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.IsActive)
                .HasColumnType("bit(1)")
                .HasColumnName("is_active");
            entity.Property(e => e.Password)
                .HasMaxLength(32)
                .HasColumnName("password")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .HasColumnName("phone")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.RoleId)
                .HasMaxLength(8)
                .HasColumnName("role_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Role_Name");
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Address");

            entity.HasIndex(e => e.ProvinceId, "FK_Address_Province");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Address1)
                .HasMaxLength(100)
                .HasColumnName("address")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ProvinceId)
                .HasMaxLength(8)
                .HasColumnName("province_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.Province).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.ProvinceId)
                .HasConstraintName("FK_Address_Province");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Category");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<ChangeReason>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Change_Reason");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Reason)
                .HasMaxLength(50)
                .HasColumnName("reason")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Comment");

            entity.HasIndex(e => e.ProductItemId, "FK_Comment_Product_Item");

            entity.HasIndex(e => e.UserId, "FK_Comment_User");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Comment1)
                .HasMaxLength(255)
                .HasColumnName("comment")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .HasColumnName("user_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

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
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Discount_Log");

            entity.HasIndex(e => e.ProductItemId, "FK_Discount_Log_Product_Item");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ChangeTimestamp)
                .HasColumnType("datetime")
                .HasColumnName("change_timestamp");
            entity.Property(e => e.NewDiscount)
                .HasPrecision(10, 2)
                .HasColumnName("new_discount");
            entity.Property(e => e.OldDiscount)
                .HasPrecision(10, 2)
                .HasColumnName("old_discount");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.DiscountLogs)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Discount_Log_Product_Item");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Order");

            entity.HasIndex(e => e.StaffShiftId, "FK_Order_Staff_Shift");

            entity.HasIndex(e => e.StateId, "FK_Order_State");

            entity.HasIndex(e => e.UserId, "FK_Order_User");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.EarnPoint)
                .HasPrecision(10, 2)
                .HasColumnName("earn_point");
            entity.Property(e => e.StaffShiftId)
                .HasMaxLength(8)
                .HasColumnName("staff_shift_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.UsePoint)
                .HasPrecision(10, 2)
                .HasColumnName("use_point");
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .HasColumnName("user_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

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

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductItemId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("Order_Item");

            entity.HasIndex(e => e.ProductItemId, "FK_Order_Item_Product_Item");

            entity.Property(e => e.OrderId)
                .HasMaxLength(8)
                .HasColumnName("order_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Discount)
                .HasPrecision(5, 2)
                .HasColumnName("discount");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Item_Order");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Item_Product_Item");
        });

        modelBuilder.Entity<OrderState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Order_State");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<PriceLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Price_Log");

            entity.HasIndex(e => e.ProductItemId, "FK_Price_Log_Product_Item");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ChangeTimestamp)
                .HasColumnType("datetime")
                .HasColumnName("change_timestamp");
            entity.Property(e => e.NewPrice)
                .HasPrecision(10, 2)
                .HasColumnName("new_price");
            entity.Property(e => e.OldPrice)
                .HasPrecision(10, 2)
                .HasColumnName("old_price");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.PriceLogs)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Price_Log_Product_Item");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Product");

            entity.HasIndex(e => e.CategoryId, "FK_Product_Category");

            entity.HasIndex(e => e.StateId, "FK_Product_State");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CategoryId)
                .HasMaxLength(8)
                .HasColumnName("category_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Picture)
                .HasMaxLength(255)
                .HasColumnName("picture")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.StateId).HasColumnName("state_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Category");

            entity.HasOne(d => d.State).WithMany(p => p.Products)
                .HasForeignKey(d => d.StateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_State");
        });

        modelBuilder.Entity<ProductConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Product_Configuration");

            entity.HasIndex(e => e.ProductItemId, "FK_Product_Configuration_Product_Item");

            entity.HasIndex(e => e.VariationOptionId, "FK_Product_Configuration_Variation_Option");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.VariationOptionId)
                .HasMaxLength(8)
                .HasColumnName("variation_option_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.ProductConfigurations)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Configuration_Product_Item");

            entity.HasOne(d => d.VariationOption).WithMany(p => p.ProductConfigurations)
                .HasForeignKey(d => d.VariationOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Configuration_Variation_Option");
        });

        modelBuilder.Entity<ProductItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Product_Item");

            entity.HasIndex(e => e.ProductId, "FK_Product_Item_Product");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Discount)
                .HasPrecision(5, 2)
                .HasColumnName("discount");
            entity.Property(e => e.ImportPrice)
                .HasPrecision(10, 2)
                .HasColumnName("import_price");
            entity.Property(e => e.ProductId)
                .HasMaxLength(8)
                .HasColumnName("product_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SellingPrice)
                .HasPrecision(10, 2)
                .HasColumnName("selling_price");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_Item_Product");
        });

        modelBuilder.Entity<ProductLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Product_Log");

            entity.HasIndex(e => e.ChangeReasonId, "FK_Product_Log_Change_Reason");

            entity.HasIndex(e => e.DisocuntLogId, "FK_Product_Log_Discount_Log");

            entity.HasIndex(e => e.PriceLogId, "FK_Product_Log_Price_Log");

            entity.HasIndex(e => e.NameLogId, "FK_Product_Log_Product_Name_Log");

            entity.HasIndex(e => e.QuantityLogId, "FK_Product_Log_Quantity_Log");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ChangeReasonId)
                .HasMaxLength(8)
                .HasColumnName("change_reason_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.DisocuntLogId)
                .HasMaxLength(8)
                .HasColumnName("disocunt_log_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.NameLogId)
                .HasMaxLength(8)
                .HasColumnName("name_log_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PriceLogId)
                .HasMaxLength(8)
                .HasColumnName("price_log_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.QuantityLogId)
                .HasMaxLength(8)
                .HasColumnName("quantity_log_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.ChangeReason).WithMany(p => p.ProductLogs)
                .HasForeignKey(d => d.ChangeReasonId)
                .HasConstraintName("FK_Product_Log_Change_Reason");

            entity.HasOne(d => d.DisocuntLog).WithMany(p => p.ProductLogs)
                .HasForeignKey(d => d.DisocuntLogId)
                .HasConstraintName("FK_Product_Log_Discount_Log");

            entity.HasOne(d => d.NameLog).WithMany(p => p.ProductLogs)
                .HasForeignKey(d => d.NameLogId)
                .HasConstraintName("FK_Product_Log_Product_Name_Log");

            entity.HasOne(d => d.PriceLog).WithMany(p => p.ProductLogs)
                .HasForeignKey(d => d.PriceLogId)
                .HasConstraintName("FK_Product_Log_Price_Log");

            entity.HasOne(d => d.QuantityLog).WithMany(p => p.ProductLogs)
                .HasForeignKey(d => d.QuantityLogId)
                .HasConstraintName("FK_Product_Log_Quantity_Log");
        });

        modelBuilder.Entity<ProductNameLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Product_Name_Log");

            entity.HasIndex(e => e.ProductItemId, "FK_Name_Log_Product_Item");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ChangeTimestamp)
                .HasColumnType("datetime")
                .HasColumnName("change_timestamp");
            entity.Property(e => e.NewName)
                .HasMaxLength(50)
                .HasColumnName("new_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.OldName)
                .HasMaxLength(50)
                .HasColumnName("old_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.ProductNameLogs)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Name_Log_Product_Item");
        });

        modelBuilder.Entity<ProductState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Product_State");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Province");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<QuantityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Quantity_Log");

            entity.HasIndex(e => e.ProductItemId, "FK_Quantity_Log_Product_Item");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ChangeTimestamp)
                .HasColumnType("datetime")
                .HasColumnName("change_timestamp");
            entity.Property(e => e.NewQuantity).HasColumnName("new_quantity");
            entity.Property(e => e.OldQuantity).HasColumnName("old_quantity");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.ProductItem).WithMany(p => p.QuantityLogs)
                .HasForeignKey(d => d.ProductItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Quantity_Log_Product_Item");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Rating");

            entity.HasIndex(e => e.ProductItemId, "FK_Rating_Product_Item");

            entity.HasIndex(e => e.UserId, "FK_Rating_User");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ProductItemId)
                .HasMaxLength(8)
                .HasColumnName("product_item_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Rating1).HasColumnName("rating");
            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .HasColumnName("user_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

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
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Role_Name");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PRIMARY");

            entity.Property(e => e.AccountId)
                .HasMaxLength(8)
                .HasColumnName("account_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Salary).HasColumnName("salary");

            entity.HasOne(d => d.Account).WithOne(p => p.Staff)
                .HasForeignKey<Staff>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Staff_Account");
        });

        modelBuilder.Entity<StaffShift>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Staff_Shift");

            entity.HasIndex(e => e.StaffId, "FK_Staff_Shift_Staff");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.ShiftTimeBegin)
                .HasColumnType("datetime")
                .HasColumnName("shift_time_begin");
            entity.Property(e => e.ShiftTimeEnd)
                .HasColumnType("datetime")
                .HasColumnName("shift_time_end");
            entity.Property(e => e.StaffId)
                .HasMaxLength(8)
                .HasColumnName("staff_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.Staff).WithMany(p => p.StaffShifts)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Staff_Shift_Staff");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PRIMARY");

            entity.ToTable("User");

            entity.Property(e => e.AccountId)
                .HasMaxLength(8)
                .HasColumnName("account_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Point).HasColumnName("point");

            entity.HasOne(d => d.Account).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Account");
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("User_Address");

            entity.HasIndex(e => e.AddressId, "FK_User_Address_Address");

            entity.Property(e => e.UserId)
                .HasMaxLength(8)
                .HasColumnName("user_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.AddressId)
                .HasMaxLength(8)
                .HasColumnName("address_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.IsDefault)
                .HasColumnType("bit(1)")
                .HasColumnName("is_default");

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
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Variation");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<VariationOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Variation_Option");

            entity.HasIndex(e => e.VariationId, "FK_Variation_Option_Variation");

            entity.Property(e => e.Id)
                .HasMaxLength(8)
                .HasColumnName("id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Value)
                .HasMaxLength(50)
                .HasColumnName("value")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.VariationId)
                .HasMaxLength(8)
                .HasColumnName("variation_id")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.Variation).WithMany(p => p.VariationOptions)
                .HasForeignKey(d => d.VariationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Variation_Option_Variation");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
