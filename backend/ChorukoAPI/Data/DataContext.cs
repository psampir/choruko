using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChorukoAPI.Models;
using System.Reflection.Emit;

namespace ChorukoAPI.Data;

public class DataContext(DbContextOptions<DataContext> options, IConfiguration Configuration) : IdentityDbContext<User>(options)
{
    public override DbSet<User> Users { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<HouseholdTask> HouseholdTasks { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Settlement> Settlements { get; set; }
    public DbSet<TaskReward> TaskRewards { get; set; }
    public DbSet<PushSubscription> PushSubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.Email).IsRequired(true);
            entity.Property(e => e.PhoneNumberConfirmed).HasDefaultValue(false);
            entity.Property(e => e.TwoFactorEnabled).HasDefaultValue(false);
            entity.Property(e => e.LockoutEnabled).HasDefaultValue(false);
            entity.Property(e => e.AccessFailedCount).HasDefaultValue(0);
            entity.Property(e => e.EmailConfirmed).HasDefaultValue(false);
        });

        builder.Entity<User>()
            .HasOne(u => u.Household)
            .WithMany(h => h.Participants)
            .HasForeignKey(u => u.HouseholdId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Household>()
            .HasOne(h => h.Admin)
            .WithOne()
            .HasForeignKey<Household>(h => h.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>()
            .HasOne(a => a.Household)
            .WithMany(h => h.Activities)
            .HasForeignKey(a => a.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<HouseholdTask>()
            .HasOne(t => t.Household)
            .WithMany(h => h.Tasks)
            .HasForeignKey(t => t.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<HouseholdTask>()
            .HasOne(t => t.Creator)
            .WithMany()
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Settlement>()
            .HasOne(s => s.Creator)
            .WithMany()
            .HasForeignKey(s => s.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<PushSubscription>()
            .HasIndex(sub => sub.Endpoint)
            .IsUnique();

        // Many-to-Many для SettledUsers
        builder.Entity<Settlement>()
            .HasMany(s => s.SettledUsers)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "SettlementSettledUsers",
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                j => j.HasOne<Settlement>().WithMany().HasForeignKey("SettlementId"));

        // Many-to-Many для UnsettledUsers
        builder.Entity<Settlement>()
            .HasMany(s => s.UnsettledUsers)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "SettlementUnsettledUsers",
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                j => j.HasOne<Settlement>().WithMany().HasForeignKey("SettlementId"));

        builder.Entity<Invitation>()
            .HasOne(i => i.Household)
            .WithMany(h => h.Invitations)
            .HasForeignKey(i => i.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Invitation>()
            .HasOne(i => i.Sender)
            .WithMany()
            .HasForeignKey(i => i.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HouseholdTask>()
            .HasMany(t => t.Assignees)
            .WithMany(u => u.AssignedTasks)
            .UsingEntity(j => j.ToTable("HouseholdTaskAssignees"));

        builder.Entity<HouseholdTask>()
            .HasIndex(t => new { t.HouseholdId, t.Status });

        builder.Entity<Settlement>()
            .HasIndex(s => new { s.HouseholdId, s.Status });
    }
}