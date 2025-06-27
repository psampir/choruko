using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChorukoAPI.Models;

namespace ChorukoAPI.Data;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<User>(options)
{
    public override DbSet<User> Users { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Household> Households { get; set; }
    public DbSet<HouseholdTask> HouseholdTasks { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Settlement> Settlements { get; set; }
    public DbSet<TaskReward> TaskRewards { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
            .HasOne(s => s.Household)
            .WithMany(h => h.Settlements)
            .HasForeignKey(s => s.HouseholdId)
            .OnDelete(DeleteBehavior.Cascade);

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