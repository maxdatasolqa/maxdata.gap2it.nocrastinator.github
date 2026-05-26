using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NoCrastinator.Api.Domain;
using NoCrastinator.Api.Services;
using NoCrastinator.Tests.Helpers;
using System.Security.Claims;

namespace NoCrastinator.Tests.Controllers
{
    public class GoalsControllerTests
    {
        [Fact]
        public void NewGoal_Defaults_AreCorrect()
        {
            var goal = new Goal();

            Assert.Equal(0, goal.ProgressPercent);
            Assert.Equal(GoalStatus.NotStarted, goal.Status);
        }

        [Fact]
        public void CompletedGoal_ShouldHave100Percent()
        {
            var goal = new Goal
            {
                Status = GoalStatus.Completed,
                ProgressPercent = 100
            };

            Assert.Equal(100, goal.ProgressPercent);
        }

        [Fact]
        public async Task UpdateProgress_ShouldCallEvaluationService()
        {
            // Arrange
            var mockService = new Mock<IGoalEvaluationService>();
            var mockContext = TestDbContextFactory.Create();
            var mockLogger = new Mock<ILogger<GoalsController>>();

            var controller = new GoalsController(
                mockContext,
                mockLogger.Object,
                mockService.Object);

            var userId = "u1";

            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = BuildUser(userId)
                }
            };

            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Test Goal",
                Description = "Test description",
                ProgressPercent = 0,
                Status = GoalStatus.NotStarted,
                Points = 10,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            mockContext.Goals.Add(goal);
            await mockContext.SaveChangesAsync();

            var request = new UpdateProgressRequest { ProgressPercent = 100 };

            // Act
            await controller.UpdateProgress(goal.Id, request);

            // Assert
            mockService.Verify(s => s.EvaluateAsync(It.IsAny<Goal>()), Times.Once);
        }
        [Fact]
        public async Task UpdateProgress_NoUser_ShouldStillProceed_Bug()
        {
            // Arrange
            var mockService = new Mock<IGoalEvaluationService>();
            var mockContext = TestDbContextFactory.Create();
            var mockLogger = new Mock<ILogger<GoalsController>>();

            var controller = new GoalsController(
                mockContext,
                mockLogger.Object,
                mockService.Object);

            var userId = "u1";


            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Test Goal",
                Description = "Test description",
                ProgressPercent = 0,
                Status = GoalStatus.NotStarted,
                Points = 10,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            mockContext.Goals.Add(goal);
            await mockContext.SaveChangesAsync();

            var request = new UpdateProgressRequest { ProgressPercent = 100 };

            // Act
            await controller.UpdateProgress(goal.Id, request);

            // Assert
            mockService.Verify(s => s.EvaluateAsync(It.IsAny<Goal>()), Times.Once);
        }
        [Fact]
        public async Task UpdateProgress_ShouldAllowUnauthorizedUser_Bug()
        {
            // Arrange
            var mockService = new Mock<IGoalEvaluationService>();
            var mockContext = TestDbContextFactory.Create();
            var mockLogger = new Mock<ILogger<GoalsController>>();

            var controller = new GoalsController(
                mockContext,
                mockLogger.Object,
                mockService.Object);

            var userId = "u2";     // acting user (NOT owner)
            var ownerId = "u1";    // actual goal owner

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = BuildUser(userId)
                }
            };

            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                UserId = ownerId, // DIFFERENT
                Title = "Test Goal",
                Description = "Test description",
                ProgressPercent = 0,
                Status = GoalStatus.NotStarted,
                Points = 10,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            mockContext.Goals.Add(goal);
            await mockContext.SaveChangesAsync();

            var request = new UpdateProgressRequest { ProgressPercent = 100 };

            // Act
            await controller.UpdateProgress(goal.Id, request);

            // Assert
            mockService.Verify(s => s.EvaluateAsync(It.IsAny<Goal>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProgress_InvalidValue_ShouldStillProceed_Bug()
        {
            var mockService = new Mock<IGoalEvaluationService>();
            var mockContext = TestDbContextFactory.Create();
            var mockLogger = new Mock<ILogger<GoalsController>>();

            var controller = new GoalsController(
                mockContext,
                mockLogger.Object,
                mockService.Object);
            var userId = "u1"; 
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = BuildUser(userId)
                }
            };

            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = "Test Goal",
                Description = "Test description",
                ProgressPercent = 0,
                Status = GoalStatus.NotStarted,
                Points = 10,
                DueDate = DateTime.UtcNow.AddDays(1)
            };
            mockContext.Goals.Add(goal);
            await mockContext.SaveChangesAsync();
            var request = new UpdateProgressRequest { ProgressPercent = 150 };

            await controller.UpdateProgress(goal.Id, request);

            mockService.Verify(s => s.EvaluateAsync(It.IsAny<Goal>()), Times.Once);
        }

        private static ClaimsPrincipal BuildUser(string userId)
        {
            var claims = new[]
            {
                new Claim("userId", userId)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }


    }
}