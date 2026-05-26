using Moq;
using NoCrastinator.Api.Domain;
using NoCrastinator.Api.Services;
using NoCrastinator.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoCrastinator.Tests.Services
{
    public class GoalEvaluationServiceTests
    {
        [Fact]
        public async Task AlreadyEvaluated_ShouldDoNothing()
        {
            var context = TestDbContextFactory.Create();

            var user = new ApplicationUser { Id = "u1", TotalPoints = 100 };
            context.Users.Add(user);

            var goal = new Goal
            {

                Title = "Test Goal",               
                Description = "Test description",  
                UserId = user.Id,
                Points = 30,
                IsEvaluated = true
            };

            context.Goals.Add(goal);
            await context.SaveChangesAsync();
            var timeProvider = new Mock<ITimeProvider>();
            var service = new GoalEvaluationService(context, timeProvider.Object);

            var delta = await service.EvaluateAsync(goal);

            Assert.Equal(0, delta);
            Assert.Equal(100, user.TotalPoints);
        }
        [Fact]
        public async Task Overdue_NotCompleted_ShouldSubtractPoints()
        {
            var context = TestDbContextFactory.Create();

            var user = new ApplicationUser { Id = "u1", TotalPoints = 100 };
            context.Users.Add(user);

            var goal = new Goal
            {

                Title = "Test Goal",               
                Description = "Test description",  
                UserId = user.Id,
                Status = GoalStatus.InProgress,
                Points = 30,
                DueDate = DateTime.UtcNow.AddDays(-1)
            };

            context.Goals.Add(goal);
            await context.SaveChangesAsync();

            var timeProvider = new Mock<ITimeProvider>();
            var service = new GoalEvaluationService(context, timeProvider.Object);

            var delta = await service.EvaluateAsync(goal);

            Assert.Equal(-30, delta);
            Assert.Equal(70, user.TotalPoints);
        }
        [Fact]
        public async Task Completed_Late_ShouldNotChangePoints()
        {
            var context = TestDbContextFactory.Create();

            var user = new ApplicationUser { Id = "u1", TotalPoints = 10 };
            context.Users.Add(user);

            var goal = new Goal
            {
                Title = "Test Goal",
                Description = "Test description",
                UserId = user.Id,
                Status = GoalStatus.Completed,
                Points = 50,
                DueDate = DateTime.UtcNow.AddDays(-1)
            };

            context.Goals.Add(goal);
            await context.SaveChangesAsync();

            var timeProvider = new Mock<ITimeProvider>();
            var service = new GoalEvaluationService(context, timeProvider.Object);

            var delta = await service.EvaluateAsync(goal);

            Assert.Equal(0, delta);
            Assert.Equal(10, user.TotalPoints);
        }
        [Fact]
        public async Task Completed_Early_ShouldAddPoints()
        {
            var context = TestDbContextFactory.Create();

            var user = new ApplicationUser { Id = "u1", TotalPoints = 0 };
            context.Users.Add(user);

            var goal = new Goal
            {
                Title = "Test Goal",
                Description = "Test description",
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Status = GoalStatus.Completed,
                Points = 50,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            context.Goals.Add(goal);
            await context.SaveChangesAsync();

            var timeProvider = new Mock<ITimeProvider>();
            var service = new GoalEvaluationService(context, timeProvider.Object);

            var delta = await service.EvaluateAsync(goal);

            Assert.Equal(50, delta);
            Assert.Equal(50, user.TotalPoints);
            Assert.True(goal.IsEvaluated);
        }
    }

}
