using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Models;

public partial class RoutedSystemContext : DbContext
{
    public RoutedSystemContext()
    {
    }

    public RoutedSystemContext(DbContextOptions<RoutedSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VehicleInfo> VehicleInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\ProjectModels;Initial Catalog=RoutedSystem;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.DriverId).HasName("PK__Driver__F1532C12678E8F8F");

            entity.ToTable("Driver");

            entity.HasIndex(e => e.NumberPlate, "UQ__Driver__DD5F458DCE740B58").IsUnique();

            entity.Property(e => e.DriverId).HasColumnName("driverID");
            entity.Property(e => e.DateOfBirth).HasColumnName("dateOfBirth");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("firstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("lastName");
            entity.Property(e => e.NumberPlate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("numberPlate");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC0774B6B1A1");

            entity.ToTable("User");

            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<VehicleInfo>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__vehicleI__5B9D25D239954AF2");

            entity.ToTable("vehicleInfo");

            entity.Property(e => e.VehicleId).HasColumnName("vehicleID");
            entity.Property(e => e.DateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("dateTime");
            entity.Property(e => e.InOrOut)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("inOrOut");
            entity.Property(e => e.NumberPlate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("numberPlate");

            entity.HasOne(d => d.NumberPlateNavigation).WithMany(p => p.VehicleInfos)
                .HasPrincipalKey(p => p.NumberPlate)
                .HasForeignKey(d => d.NumberPlate)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__vehicleIn__numbe__4AB81AF0");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
