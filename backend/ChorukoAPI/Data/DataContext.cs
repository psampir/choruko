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
}