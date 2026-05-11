using BADEAPORTAL.Models;
using BADEAPORTAL.Models.Documents;

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
        public DbSet<PortalHeroSlide> PortalHeroSlides => Set<PortalHeroSlide>();
        public DbSet<PortalDocument> PortalDocuments => Set<PortalDocument>();

        public DbSet<PortalDocumentVersion> PortalDocumentVersions => Set<PortalDocumentVersion>();
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
            modelBuilder.Entity<PortalHeroSlide>(e =>
{
    e.ToTable("PORTAL_HERO_SLIDES");

    e.HasKey(x => x.SlideId);

    e.Property(x => x.SlideId)
        .HasColumnName("SLIDE_ID")
        .ValueGeneratedOnAdd();

    e.Property(x => x.ImagePath)
        .HasColumnName("IMAGE_PATH")
        .HasMaxLength(500)
        .IsRequired();

    e.Property(x => x.AltTextEn)
        .HasColumnName("ALT_TEXT_EN")
        .HasMaxLength(200);

    e.Property(x => x.AltTextAr)
        .HasColumnName("ALT_TEXT_AR")
        .HasMaxLength(200);

    e.Property(x => x.SortOrder)
        .HasColumnName("SORT_ORDER")
        .HasDefaultValue(0)
        .IsRequired();

    e.Property(x => x.IsActive)
        .HasColumnName("IS_ACTIVE")
        .HasPrecision(1, 0)
        .HasDefaultValue(1)
        .IsRequired();

    e.Property(x => x.CreatedAt)
        .HasColumnName("CREATED_AT")
        .IsRequired();

    e.Property(x => x.UpdatedAt)
        .HasColumnName("UPDATED_AT");
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
            modelBuilder.Entity<PortalDocument>(entity =>
{
    entity.ToTable("PORTAL_DOCUMENTS", "STRATEGYKPI");

    entity.HasKey(x => x.DocumentId);

    entity.Property(x => x.DocumentId)
        .HasColumnName("DOCUMENT_ID");

    entity.Property(x => x.Name)
        .HasColumnName("NAME")
        .HasMaxLength(300)
        .IsRequired();

    entity.Property(x => x.Description)
        .HasColumnName("DESCRIPTION")
        .HasMaxLength(1000);

    entity.Property(x => x.FolderPath)
        .HasColumnName("FOLDER_PATH")
        .HasMaxLength(500);

    entity.Property(x => x.Status)
        .HasColumnName("STATUS");

    entity.Property(x => x.CreatedBy)
        .HasColumnName("CREATED_BY")
        .HasMaxLength(256);

    entity.Property(x => x.CreatedAt)
        .HasColumnName("CREATED_AT");

    entity.Property(x => x.LastUpdatedBy)
        .HasColumnName("LAST_UPDATED_BY")
        .HasMaxLength(256);

    entity.Property(x => x.LastUpdatedAt)
        .HasColumnName("LAST_UPDATED_AT");
});
            modelBuilder.Entity<PortalDocumentVersion>(entity =>
            {
                entity.ToTable("PORTAL_DOCUMENT_VERSIONS", "STRATEGYKPI");

                entity.HasKey(x => x.VersionId);

                entity.Property(x => x.VersionId)
                    .HasColumnName("VERSION_ID");

                entity.Property(x => x.DocumentId)
                    .HasColumnName("DOCUMENT_ID");

                entity.Property(x => x.VersionNo)
                    .HasColumnName("VERSION_NO");

                entity.Property(x => x.OriginalFileName)
                    .HasColumnName("ORIGINAL_FILE_NAME")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.FileType)
                    .HasColumnName("FILE_TYPE")
                    .HasMaxLength(50);

                entity.Property(x => x.SharePointItemId)
                    .HasColumnName("SHAREPOINT_ITEM_ID")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.FileSize)
                    .HasColumnName("FILE_SIZE");

                entity.Property(x => x.UploadedBy)
                    .HasColumnName("UPLOADED_BY")
                    .HasMaxLength(256);

                entity.Property(x => x.UploadedAt)
                    .HasColumnName("UPLOADED_AT");

                entity.Property(x => x.IsCurrent)
                    .HasColumnName("IS_CURRENT");

                entity.Property(x => x.FilePath)
                    .HasColumnName("FILE_PATH")
                    .HasMaxLength(1000);

                entity.HasOne(x => x.Document)
                    .WithMany(x => x.Versions)
                    .HasForeignKey(x => x.DocumentId);
            });
        }
    }
}
