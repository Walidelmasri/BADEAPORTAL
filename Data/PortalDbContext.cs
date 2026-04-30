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

        // These will be used for pickers (read-only)
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<PortalSystemCard> PortalSystemCards => Set<PortalSystemCard>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // ANNOUNCEMENTS
            // =========================
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
                    .HasConversion<int>()          // IMPORTANT for Oracle
                    .HasDefaultValue(1)             // default = memo
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

                // sender (2B)
                a.Property(x => x.FromKind)
                    .HasColumnName("FROM_KIND")
                    .HasMaxLength(10)
                    .HasDefaultValue("USER")
                    .IsRequired();

                a.Property(x => x.FromDeptCode)
                    .HasColumnName("FROM_DEPT_CODE")
                    .HasMaxLength(3);
                a.Property(x => x.ToKind).HasColumnName("TO_KIND").HasMaxLength(10);
                a.Property(x => x.ToDeptCode).HasColumnName("TO_DEPT_CODE").HasMaxLength(30);
                // notifications
                a.Property(x => x.NotifyInApp)
                    .HasColumnName("NOTIFY_INAPP")
                    .HasColumnType("NUMBER(1)")
                    .HasConversion<int>()
                    .HasDefaultValue(1)
                    .IsRequired();

                a.Property(x => x.NotifyEmail)
                    .HasColumnName("NOTIFY_EMAIL")
                    .HasColumnType("NUMBER(1)")
                    .HasConversion<int>()
                    .HasDefaultValue(1)
                    .IsRequired();
            });
            modelBuilder.Entity<PortalSystemCard>(e =>
            {
                e.ToTable("PORTAL_SYSTEM_CARDS");

                e.HasKey(x => x.CardId);

                e.Property(x => x.CardId).HasColumnName("CARD_ID").ValueGeneratedOnAdd();
                e.Property(x => x.SysId).HasColumnName("SYSID");

                e.Property(x => x.SysNameEn).HasColumnName("SYSNAME_EN").HasMaxLength(150).IsRequired();
                e.Property(x => x.SysNameAr).HasColumnName("SYSNAME_AR").HasMaxLength(150).IsRequired();

                e.Property(x => x.DescriptionEn).HasColumnName("DESCRIPTION_EN").HasMaxLength(500);
                e.Property(x => x.DescriptionAr).HasColumnName("DESCRIPTION_AR").HasMaxLength(500);

                e.Property(x => x.CategoryEn).HasColumnName("CATEGORY_EN").HasMaxLength(100);
                e.Property(x => x.CategoryAr).HasColumnName("CATEGORY_AR").HasMaxLength(100);

                e.Property(x => x.AppUrl).HasColumnName("APP_URL").HasMaxLength(1000).IsRequired();
                e.Property(x => x.LogoPath).HasColumnName("LOGO_PATH").HasMaxLength(500);

                e.Property(x => x.RoleGroup).HasColumnName("ROLE_GROUP").HasMaxLength(100);

                e.Property(x => x.IsPublic)
                    .HasColumnName("IS_PUBLIC")
                    .HasPrecision(1, 0)
                    .HasDefaultValue(1)
                    .IsRequired();

                e.Property(x => x.IsActive)
                    .HasColumnName("IS_ACTIVE")
                    .HasPrecision(1, 0)
                    .HasDefaultValue(1)
                    .IsRequired();

                e.Property(x => x.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
                e.Property(x => x.UpdatedAt).HasColumnName("UPDATED_AT");
            });
            // =========================
            // EMPLOYEES (READ-ONLY)
            // =========================
            modelBuilder.Entity<Employee>(e =>
            {
                e.ToTable("EMPLOYEES", "BADEA_ADDONS");
                e.HasKey(x => x.EmpId);

            });

            // =========================
            // DEPARTMENTS
            // =========================
            modelBuilder.Entity<Department>(d =>
            {
                d.ToTable("DEPARTMENTS");
                d.HasKey(x => x.DeptCode);
            });
        }
    }
}
