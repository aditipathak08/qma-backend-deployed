using Microsoft.EntityFrameworkCore;
using QuantityMeasurementApp.Models;

namespace QuantityMeasurementApp.Repository
{
    public class QuantityMeasurementDbContext : DbContext
    {
        public QuantityMeasurementDbContext(DbContextOptions<QuantityMeasurementDbContext> options)
            : base(options)
        {
        }

        public DbSet<QuantityMeasurementEntity> QuantityMeasurements { get; set; }
        public DbSet<UserEntity> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").IsRequired();
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
                entity.Property(e => e.Salt).HasColumnName("salt").IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<QuantityMeasurementEntity>(entity =>
            {
                entity.ToTable("quantity_measurements");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Operation).HasColumnName("operation").IsRequired();
                entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
                
                entity.OwnsOne(e => e.Operand1, o =>
                {
                    o.Property(p => p.Value).HasColumnName("operand1_value");
                    o.Property(p => p.Unit).HasColumnName("operand1_unit");
                });

                entity.OwnsOne(e => e.Operand2, o =>
                {
                    o.Property(p => p.Value).HasColumnName("operand2_value");
                    o.Property(p => p.Unit).HasColumnName("operand2_unit");
                });

                entity.OwnsOne(e => e.Result, o =>
                {
                    o.Property(p => p.Value).HasColumnName("result_value");
                    o.Property(p => p.Unit).HasColumnName("result_unit");
                });
            });
        }
    }
}
