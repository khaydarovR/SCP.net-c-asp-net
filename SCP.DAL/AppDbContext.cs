using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SCP.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

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
        public DbSet<SafeUsersClaim> SafeClaims { get; set; }
        public DbSet<SafeUsers> SafeUsers { get; set; }
        public DbSet<RecUsers> RecUsers { get; set; }
        public DbSet<Rec> Records { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<WhiteIPList> WhiteIPs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var appUserBuilder = modelBuilder.Entity<AppUser>();
            appUserBuilder.HasKey(c => c.Id);
            appUserBuilder.HasIndex(c => c.Id).IsUnique();

            modelBuilder.Entity<SafeUsers>()
                .HasKey(su => su.Id);

            modelBuilder.Entity<RecUsers>()
                .HasKey(ru => new { ru.RecordId, ru.AppUserId });
            modelBuilder.Entity<RecUsers>()
                .Property(c => c.Right).IsRequired();

            var safeBuilder = modelBuilder.Entity<Safe>();
            safeBuilder.HasKey(c => c.Id);
            safeBuilder.HasIndex(c => c.Id).IsUnique();
            safeBuilder.Property(c => c.Title).IsRequired();
            safeBuilder.Property(c => c.Description).IsRequired(false);


            var recordBuilder = modelBuilder.Entity<Rec>();
            recordBuilder.HasKey(c => c.Id);
            recordBuilder.HasIndex(c => c.Id).IsUnique();
            recordBuilder.Property(c => c.Title).IsRequired();
            recordBuilder.Property(c => c.Login).IsRequired();
            recordBuilder.Property(c => c.Pw).IsRequired();
            recordBuilder.Property(c => c.Secret).IsRequired(false);
            recordBuilder.Property(c => c.IsDeleted).HasDefaultValue(false);
            recordBuilder.HasOne(c => c.Safe)
                .WithMany(s => s.Records)
                .HasForeignKey(c => c.SafeId)
                .OnDelete(DeleteBehavior.Cascade);

            var changesBuilder = modelBuilder.Entity<ActivityLog>();
            changesBuilder.HasKey(c => c.Id);
            changesBuilder.HasIndex(c => c.Id).IsUnique();
            changesBuilder.Property(c => c.Text).IsRequired();
            changesBuilder.Property(c => c.At).HasDefaultValue(DateTime.UtcNow);
            changesBuilder.HasOne(c => c.Record)
                .WithMany(s => s.ActivityLog)
                .HasForeignKey(c => c.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
            changesBuilder.HasOne(c => c.AppUser)
                .WithMany(s => s.ChangerHistory)
                .HasForeignKey(c => c.AppUsreId)
                .OnDelete(DeleteBehavior.Cascade);

            var whiteIpsBuilder = modelBuilder.Entity<WhiteIPList>();
            whiteIpsBuilder.HasIndex(c => c.Id).IsUnique();
            whiteIpsBuilder.Property(c => c.WhiteIp).IsRequired();
            whiteIpsBuilder.HasOne(c => c.AppUser)
                .WithMany(s => s.WhiteIPs)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);


            var safeClaimsBuilder = modelBuilder.Entity<SafeUsersClaim>();
            safeClaimsBuilder.HasOne(c => c.UserForSafe)
                .WithMany(s => s.Claims)
                .HasForeignKey(c => c.UserForSafeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
