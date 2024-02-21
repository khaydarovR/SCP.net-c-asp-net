using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCP.Domain.Entity;

namespace SCP.DAL
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid,
                                                IdentityUserClaim<Guid>,
                                                IdentityUserRole<Guid>,
                                                IdentityUserLogin<Guid>,
                                                IdentityRoleClaim<Guid>,
                                                IdentityUserToken<Guid>>
    {
        public DbSet<AppUser> AppUsers { get; set; }

        public DbSet<Safe> Safes { get; set; }
        public DbSet<SafeRight> SafeRights { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<RecordRight> RecordRights { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<UserWhiteIP> UserWhiteIPs { get; set; }
        public DbSet<ApiKeyWhiteIP> ApiKeyWhiteIPs { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var appUserModelBuilder = modelBuilder.Entity<AppUser>();
            ConfigureAppUser(appUserModelBuilder);


            var botModelBuilder = modelBuilder.Entity<ApiKey>();
            ConfigureApiKey(botModelBuilder);

            var botRightModelBuilder = modelBuilder.Entity<ApiKeyWhiteIP>();
            ConfigureBotRight(botRightModelBuilder);


            var safeBuilder = modelBuilder.Entity<Safe>();
            ConfigureSafe(safeBuilder);

            var safeRightModelBuilder = modelBuilder.Entity<SafeRight>();
            ConfigureSafeRight(safeRightModelBuilder);


            var recordModelBuilder = modelBuilder.Entity<Record>();
            ConfigureRecord(recordModelBuilder);

            var recordRightBuilder = modelBuilder.Entity<RecordRight>();
            ConfigureRecordRight(recordRightBuilder);


            var activityLogBuilder = modelBuilder.Entity<ActivityLog>();
            ConfigureActivityLog(activityLogBuilder);


            var userWhiteIPBuilder = modelBuilder.Entity<UserWhiteIP>();
            ConfigureUserWhiteIps(userWhiteIPBuilder);

            var botWhiteIpModelBuilder = modelBuilder.Entity<ApiKeyWhiteIP>();
            ConfigureApiIP(botWhiteIpModelBuilder);

        }

        private static void ConfigureUserWhiteIps(EntityTypeBuilder<UserWhiteIP> userWhiteIPBuilder)
        {
            // Key and index configurations
            _ = userWhiteIPBuilder.HasKey(uwip => uwip.Id);
            _ = userWhiteIPBuilder.HasIndex(uwip => uwip.Id).IsUnique();

            // Property configurations
            _ = userWhiteIPBuilder.Property(uwip => uwip.AllowFrom).IsRequired();

            // Relationship configurations
            _ = userWhiteIPBuilder.HasOne(uwip => uwip.AppUser)
                              .WithMany(au => au.WhiteIPs)
                              .HasForeignKey(uwip => uwip.AppUserId)
                              .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureActivityLog(EntityTypeBuilder<ActivityLog> activityLogBuilder)
        {
            // Key and index configurations
            _ = activityLogBuilder.HasKey(a => a.Id);
            _ = activityLogBuilder.HasIndex(a => a.Id).IsUnique();

            // Property configurations
            _ = activityLogBuilder.Property(a => a.At).IsRequired();
            _ = activityLogBuilder.Property(a => a.LogText).IsRequired();

            // Relationship configurations
            _ = activityLogBuilder.HasOne(a => a.Record)
                              .WithMany(r => r.ActivityLogs)
                              .HasForeignKey(a => a.RecordId)
                              .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureRecordRight(EntityTypeBuilder<RecordRight> recordRightBuilder)
        {
            // Setting primary key
            _ = recordRightBuilder.HasKey(r => r.Id);

            // Setting uniqueness of Id
            _ = recordRightBuilder.HasIndex(r => r.Id).IsUnique();

        }

        private static void ConfigureSafe(EntityTypeBuilder<Safe> safeBuilder)
        {
            // Key and index configurations
            _ = safeBuilder.HasKey(s => s.Id);
            _ = safeBuilder.HasIndex(s => s.Id).IsUnique();

            // Property configurations
            _ = safeBuilder.Property(s => s.Title).IsRequired();
            _ = safeBuilder.Property(s => s.Description).IsRequired(false);
            _ = safeBuilder.Property(s => s.EPrivateKpem).IsRequired(false);
            _ = safeBuilder.Property(s => s.PublicKpem).IsRequired(false);

            // Relationship configurations
            _ = safeBuilder.HasMany(s => s.Records)
                       .WithOne()
                       .HasForeignKey(r => r.SafeId)
                       .OnDelete(DeleteBehavior.Cascade);
            _ = safeBuilder.HasMany(s => s.SafeUsers)
                       .WithOne()
                       .HasForeignKey(su => su.SafeId)
                       .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureAppUser(EntityTypeBuilder<AppUser> appUserModelBuilder)
        {

            // Property settings
            _ = appUserModelBuilder.Property(au => au.CreatedDate)
                               .IsRequired()
                               .HasDefaultValue(DateTime.UtcNow);

            // Relationships
            _ = appUserModelBuilder.HasMany(au => au.SafeRights)
                               .WithOne()
                               .HasForeignKey(su => su.AppUserId)
                               .OnDelete(DeleteBehavior.Cascade);

            _ = appUserModelBuilder.HasMany(au => au.RecordRights)
                               .WithOne()
                               .HasForeignKey(ru => ru.AppUserId)
                               .OnDelete(DeleteBehavior.Cascade);

            _ = appUserModelBuilder.HasMany(au => au.WhiteIPs)
                               .WithOne()
                               .HasForeignKey(wip => wip.AppUserId)
                               .OnDelete(DeleteBehavior.Cascade);

            _ = appUserModelBuilder.HasMany(au => au.ApiKeys)
                               .WithOne()
                               .HasForeignKey(b => b.Id)
                               .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureSafeRight(EntityTypeBuilder<SafeRight> safeRightModelBuilder)
        {
            // Setting primary key
            _ = safeRightModelBuilder.HasKey(r => r.Id);

            // Setting uniqueness of Id
            _ = safeRightModelBuilder.HasIndex(r => r.Id).IsUnique();

            // Properties settings
            _ = safeRightModelBuilder.Property(sr => sr.Permission)
                                 .IsRequired()
                                 .HasMaxLength(200);

            //if DeadDate is null -> permission for perpetual rights
            _ = safeRightModelBuilder.Property(sr => sr.DeadDate)
                                 .IsRequired(false);
        }

        private static void ConfigureBotRight(EntityTypeBuilder<ApiKeyWhiteIP> botRightModelBuilder)
        {
            // Setting primary key
            _ = botRightModelBuilder.HasKey(r => r.Id);

            // Setting uniqueness of Id
            _ = botRightModelBuilder.HasIndex(r => r.Id).IsUnique();


        }

        private static void ConfigureRecord(EntityTypeBuilder<Record> recordModelBuilder)
        {

            // Setting primary key
            _ = recordModelBuilder.HasKey(r => r.Id);

            // Setting uniqueness of Id
            _ = recordModelBuilder.HasIndex(r => r.Id).IsUnique();

            // Properties settings
            _ = recordModelBuilder.Property(r => r.Title)
                              .IsRequired()
                              .HasMaxLength(100);
            _ = recordModelBuilder.Property(r => r.ELogin)
                              .IsRequired(false)
                              .HasMaxLength(500);
            _ = recordModelBuilder.Property(r => r.EPw)
                              .IsRequired(false)
                              .HasMaxLength(500);
            _ = recordModelBuilder.Property(r => r.ESecret)
                              .HasMaxLength(500);
            _ = recordModelBuilder.Property(r => r.ForResource)
                              .IsRequired(false)
                              .HasMaxLength(200);

            // Setting default value for IsDeleted
            _ = recordModelBuilder.Property(r => r.IsDeleted)
                              .HasDefaultValue(false);

            // Relationships settings
            _ = recordModelBuilder.HasOne(r => r.Safe)
                              .WithMany(s => s.Records)
                              .HasForeignKey(r => r.SafeId)
                              .OnDelete(DeleteBehavior.Cascade);

        }

        private static void ConfigureApiIP(EntityTypeBuilder<ApiKeyWhiteIP> botWhiteIpModelBuilder)
        {
            // Setting primary key
            _ = botWhiteIpModelBuilder.HasKey(b => b.Id);

            // Setting uniqueness of Id
            _ = botWhiteIpModelBuilder.HasIndex(b => b.Id).IsUnique();

            // Properties settings
            _ = botWhiteIpModelBuilder.Property(b => b.AllowFrom)
                                  .IsRequired()
                                  .HasMaxLength(100);

            _ = botWhiteIpModelBuilder.Property(b => b.AllowFrom)
                      .IsRequired()
                      .HasMaxLength(100);

            // Relationships settings
            _ = botWhiteIpModelBuilder.HasOne(b => b.ApiKey)
                                  .WithMany(b => b.WhiteIPs)
                                  .HasForeignKey(b => b.ApiKeyId)
                                  .OnDelete(DeleteBehavior.Cascade);

        }

        private static void ConfigureApiKey(EntityTypeBuilder<ApiKey> botModelBuilder)
        {
            // Setting primary key
            _ = botModelBuilder.HasKey(b => b.Id);

            // Setting uniqueness of Id
            _ = botModelBuilder.HasIndex(b => b.Id).IsUnique();

            // Properties settings
            _ = botModelBuilder.Property(b => b.Name)
                           .IsRequired()
                           .HasMaxLength(100);

            _ = botModelBuilder.Property(b => b.DeadDate)
                           .IsRequired();

            // Relationships settings
            _ = botModelBuilder.HasOne(b => b.Owner)
                           .WithMany(u => u.ApiKeys)
                           .HasForeignKey(b => b.OwnerId)
                           .OnDelete(DeleteBehavior.Cascade);


            _ = botModelBuilder
                .HasOne(a => a.Safe)
                .WithMany(s => s.ApiKeys)
                .HasForeignKey(a => a.SafeId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
