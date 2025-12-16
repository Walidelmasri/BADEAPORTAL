using BADEAPORTAL.Models;

using Microsoft.EntityFrameworkCore;

namespace BADEAPORTAL.Data
{
    public class PortalDbContext : DbContext
    {
        public PortalDbContext(DbContextOptions<PortalDbContext> options)
            : base(options)
        {
        }

        public DbSet<Announcement> Announcements => Set<Announcement>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Announcement>(a =>
    {
        a.ToTable("ANNOUNCEMENTS");

        a.HasKey(x => x.Id);

        a.Property(x => x.Id)
            .HasColumnName("ID")
            .ValueGeneratedOnAdd();

        a.Property(x => x.Title)
            .HasColumnName("TITLE")
            .HasMaxLength(200)
            .IsRequired();

        a.Property(x => x.BodyHtml)
            .HasColumnName("BODYHTML")
            .HasColumnType("CLOB")
            .IsRequired();

        a.Property(x => x.IsMemo)
            .HasColumnName("ISMEMO")
            .HasColumnType("NUMBER(1)")
            .HasDefaultValue(false)
            .IsRequired();

        a.Property(x => x.MemoTo)
            .HasColumnName("MEMOTO")
            .HasMaxLength(200);

        a.Property(x => x.MemoThrough)
            .HasColumnName("MEMOTHROUGH")
            .HasMaxLength(200);

        a.Property(x => x.MemoFrom)
            .HasColumnName("MEMOFROM")
            .HasMaxLength(200);

        a.Property(x => x.MemoSubject)
            .HasColumnName("MEMOSUBJECT")
            .HasMaxLength(200);

        a.Property(x => x.MemoClassification)
            .HasColumnName("MEMOCLASSIFICATION")
            .HasMaxLength(100);

        a.Property(x => x.CreatedAtUtc)
            .HasColumnName("CREATEDATUTC")
            .IsRequired();

        a.Property(x => x.CreatedByName)
            .HasColumnName("CREATEDBYNAME")
            .HasMaxLength(200)
            .IsRequired();

        a.Property(x => x.CreatedByUpn)
            .HasColumnName("CREATEDBYUPN")
            .HasMaxLength(256)
            .IsRequired();
    });
}

    }
}
