using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
        public DbSet<RecordRight> RecordRights { get; set; }
        public DbSet<Record> Records { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<WhiteIPList> WhiteIPs { get; set; }
        public DbSet<Bot> Bots { get; set;  }
        public DbSet<BotRight> BotRights { get; set;  }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var appUserBuilder = modelBuilder.Entity<AppUser>();
            appUserBuilder.HasKey(c => c.Id);
            appUserBuilder.HasIndex(c => c.Id).IsUnique();

            modelBuilder.Entity<SafeRight>()
                .HasKey(sr => new {sr.SafeId, sr.AppUserId, sr.ClaimValue});
            modelBuilder.Entity<SafeRight>()
                .Property(rc => rc.ClaimValue);


            var botModelBuilder = modelBuilder.Entity<Bot>();
            appUserBuilder.HasKey(c => c.Id);
            appUserBuilder.HasIndex(c => c.Id).IsUnique();

            modelBuilder.Entity<BotRight>()
                .HasKey(br => new { br.SafeId, br.BotId, br.ClaimValue });
            modelBuilder.Entity<SafeRight>()
                .Property(rc => rc.ClaimValue);

            modelBuilder.Entity<RecordRight>()
                .HasKey(ru => new { ru.RecordId, ru.AppUserId });
            modelBuilder.Entity<RecordRight>()
                .Property(c => c.Right).IsRequired();

            var safeBuilder = modelBuilder.Entity<Safe>();
            safeBuilder.HasKey(c => c.Id);
            safeBuilder.HasIndex(c => c.Id).IsUnique();
            safeBuilder.Property(c => c.Title).IsRequired();
            safeBuilder.Property(c => c.Description).IsRequired(false);
            safeBuilder.Property(c => c.PrivateK).IsRequired(false);
            safeBuilder.Property(c => c.PublicK).IsRequired(false);


            var recordBuilder = modelBuilder.Entity<Record>();
            recordBuilder.HasKey(c => c.Id);
            recordBuilder.HasIndex(c => c.Id).IsUnique();
            recordBuilder.Property(c => c.Title).IsRequired();
            recordBuilder.Property(c => c.ELogin).IsRequired();
            recordBuilder.Property(c => c.EPw).IsRequired();
            recordBuilder.Property(c => c.ESecret).IsRequired(false);
            recordBuilder.Property(c => c.IsDeleted).HasDefaultValue(false);
            recordBuilder.HasOne(c => c.Safe)
                .WithMany(s => s.Records)
                .HasForeignKey(c => c.SafeId)
                .OnDelete(DeleteBehavior.Cascade);

            var changesBuilder = modelBuilder.Entity<ActivityLog>();
            changesBuilder.HasKey(c => c.Id);
            changesBuilder.HasIndex(c => c.Id).IsUnique();
            changesBuilder.Property(c => c.LogText).IsRequired();
            changesBuilder.Property(c => c.At).HasDefaultValue(DateTime.UtcNow);
            changesBuilder.HasOne(c => c.Record)
                .WithMany(s => s.ActivityLog)
                .HasForeignKey(c => c.RecordId)
                .OnDelete(DeleteBehavior.Cascade);

            var whiteIpsBuilder = modelBuilder.Entity<WhiteIPList>();
            whiteIpsBuilder.HasIndex(c => c.Id).IsUnique();
            whiteIpsBuilder.Property(c => c.WhiteIp).IsRequired();
            whiteIpsBuilder.HasOne(c => c.AppUser)
                .WithMany(s => s.WhiteIPs)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);


            
        }
    }
}
