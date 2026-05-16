using global::SecureGate.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecureGate.Web.Models.Auth;

namespace SecureGate.Web.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Users> Students => Set<Users>();
        public DbSet<Staff> StaffMembers => Set<Staff>();
        public DbSet<Turnstile> Turnstiles => Set<Turnstile>();
        public DbSet<TurnstilePermission> TurnstilePermissions => Set<TurnstilePermission>();
        public DbSet<Camera> Cameras => Set<Camera>();
        public DbSet<CameraGroup> CameraGroups => Set<CameraGroup>();
        public DbSet<AccessLog> AccessLogs => Set<AccessLog>();
        public DbSet<FaceData> FaceData => Set<FaceData>();
        public DbSet<BlockedUser> BlockedUsers => Set<BlockedUser>();
        public DbSet<Alert> Alerts => Set<Alert>();
        public DbSet<Setting> Settings => Set<Setting>();
        public DbSet<UserPermission> UserPermissions => Set<UserPermission>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<Users>()
                .HasIndex(s => s.StudentId)
                .IsUnique();

            modelBuilder.Entity<Setting>()
                .HasIndex(s => s.Key)
                .IsUnique();

            // Student -> BlockedUser (one-to-one)
            modelBuilder.Entity<Users>()
                .HasOne(s => s.BlockedUser)
                .WithOne(b => b.Student)
                .HasForeignKey<BlockedUser>(b => b.StudentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Cascade delete prevention
            modelBuilder.Entity<AccessLog>()
                .HasOne(a => a.Turnstile)
                .WithMany(t => t.AccessLogs)
                .HasForeignKey(a => a.TurnstileId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AccessLog>()
                .HasOne(a => a.Student)
                .WithMany(s => s.AccessLogs)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Turnstile>()
                .HasOne(t => t.LinkedCamera)
                .WithMany(c => c.LinkedTurnstiles)
                .HasForeignKey(t => t.LinkedCameraId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bir foydalanuvchi faqat bitta yuz profiliga ega bo'lishi uchun
            // Student uchun Unique Index (bir talabaga bitta yuz)
            modelBuilder.Entity<FaceData>()
                .HasIndex(f => f.StudentId)
                .IsUnique()
                .HasFilter("[StudentId] IS NOT NULL");

            // Teacher uchun Unique Index
            modelBuilder.Entity<FaceData>()
                .HasIndex(f => f.TeacherId)
                .IsUnique()
                .HasFilter("[TeacherId] IS NOT NULL");

            // UserPermission konfiguratsiyasi
            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermission>()
                .HasIndex(up => new { up.UserId, up.Permission })
                .IsUnique();
        }
    }
}
