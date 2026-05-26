using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NoCrastinator.Api.Domain;

public class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        // ✅ Ensure DB created
        await context.Database.MigrateAsync();

        // ✅ Check if already seeded
        if (await userManager.Users.AnyAsync() || context.Goals.Any())
        {
            return;
        }

        // 👤 Create Users
        var user1 = new ApplicationUser
        {
            UserName = "user1@test.com",
            Email = "user1@test.com",
            TotalPoints = 0
        };

        var user2 = new ApplicationUser
        {
            UserName = "user2@test.com",
            Email = "user2@test.com",
            TotalPoints = 0
        };

        await userManager.CreateAsync(user1, "Password123!");
        await userManager.CreateAsync(user2, "Password123!");

        // Reload users (to ensure IDs)
        user1 = await userManager.FindByEmailAsync("user1@test.com");
        user2 = await userManager.FindByEmailAsync("user2@test.com");

        // 🎯 Create Goals
        var goals = new List<Goal>
        {
            new Goal
            {
                Id = Guid.NewGuid(),
                Title = "Finish API MVP",
                Description = "Complete NoCrastinator backend",
                Points = 50,
                DueDate = DateTime.UtcNow.AddDays(3),
                Status = GoalStatus.InProgress,
                ProgressPercent = 40,
                UserId = user1.Id
            },
            new Goal
            {
                Id = Guid.NewGuid(),
                Title = "Start UI",
                Description = "Build basic React or Razor UI",
                Points = 30,
                DueDate = DateTime.UtcNow.AddDays(5),
                Status = GoalStatus.NotStarted,
                ProgressPercent = 0,
                UserId = user1.Id
            },
            new Goal
            {
                Id = Guid.NewGuid(),
                Title = "Test Reward Logic",
                Description = "Validate reward/punishment rules",
                Points = 40,
                DueDate = DateTime.UtcNow.AddDays(-1), // overdue
                Status = GoalStatus.InProgress,
                ProgressPercent = 20,
                UserId = user2.Id
            }
        };

        context.Goals.AddRange(goals);
        await context.SaveChangesAsync();
    }
}