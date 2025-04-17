using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NET_base.Models.Common;

namespace API.Models.Context
{
    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<Donation> Donations { get; set; } = null!;
        public virtual DbSet<UserDonation> UserDonations { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                string connectionString = configuration.GetConnectionString("mysql");

                /*optionsBuilder.UseNpgsql(connectionString);*/
                optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 27)));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");

                entity.HasIndex(e => e.UserId, "FK_user");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.IsRead)
                    .HasColumnName("is_read")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Message)
                    .HasColumnType("text")
                    .HasColumnName("message");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_user");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RoleName)
                    .HasColumnName("role_name")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(e => e.Email, "email")
                    .IsUnique();

                entity.HasIndex(e => e.Username, "username")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasColumnType("text")
                    .HasColumnName("address");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Email).HasColumnName("email");

                entity.Property(e => e.FullName)
                    .HasMaxLength(255)
                    .HasColumnName("full_name");

                entity.Property(e => e.IsDeleted)
                    .HasColumnName("is_deleted")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .HasColumnName("password");

                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .HasColumnName("phone");

                entity.Property(e => e.Role).HasColumnName("role");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("updated_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.LastLogin)
                    .HasColumnType("timestamp");

                entity.Property(e => e.Username)
                    .HasMaxLength(100)
                    .HasColumnName("username");

                entity.Property(e => e.Token)
                    .HasMaxLength(20)
                    .HasColumnName("token");

                entity.HasOne(d => d.RoleNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.Role)
                    .HasConstraintName("FK_user_role");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transactions");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.OrderId).HasColumnName("order_id");
                entity.Property(t => t.UserId).HasColumnName("user_id");
                entity.Property(t => t.Amount).HasColumnName("amount")
                    .HasColumnType("decimal(18, 2)");
                entity.Property(t => t.Status).HasColumnName("status")
                    .HasDefaultValue(Constant.PENDING)
                    .HasMaxLength(50);
                entity.Property(t => t.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Donation>(entity =>
            {
                entity.ToTable("donations");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Description).HasColumnName("description").HasColumnType("text");
                entity.Property(e => e.GoalAmount).HasColumnName("goal_amount").HasColumnType("decimal(18, 2)");
                entity.Property(e => e.RaisedAmount).HasColumnName("raised_amount").HasColumnType("decimal(18, 2)").HasDefaultValue(0);
                entity.Property(e => e.StartDate).HasColumnName("start_date").HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnName("end_date").HasColumnType("datetime");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue(Constant.NEW);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            });

            modelBuilder.Entity<UserDonation>(entity =>
            {
                entity.ToTable("user_donations");

                entity.HasKey(e => new { e.UserId, e.DonationId });

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.DonationId).HasColumnName("donation_id");
                entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(18, 2)");
                entity.Property(e => e.DonationDate).HasColumnName("donation_date").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserDonations)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Donation)
                    .WithMany(p => p.UserDonations)
                    .HasForeignKey(d => d.DonationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
