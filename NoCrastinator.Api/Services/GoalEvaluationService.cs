using NoCrastinator.Api.Domain;

namespace NoCrastinator.Api.Services
{
    public class GoalEvaluationService : IGoalEvaluationService
    {
        private readonly AppDbContext _context;
        private readonly ITimeProvider _time;
        public GoalEvaluationService(AppDbContext context, ITimeProvider time)
        {
            _context = context;
            _time = time;   
        }

        public async Task<int> EvaluateAsync(Goal goal)
        {
            if (goal.IsEvaluated)
                return 0;

            var user = await _context.Users.FindAsync(goal.UserId);
            if (user == null) return 0;

            var now = DateTime.UtcNow;

            int delta = 0;

            if (goal.Status == GoalStatus.Completed)
            {
                if (goal.DueDate > now)
                    delta = goal.Points;
            }
            else if (goal.DueDate < now)
            {
                delta = -goal.Points;
            }

            user.TotalPoints += delta;
            goal.IsEvaluated = true;

            await _context.SaveChangesAsync();

            return delta;
        }
    }
}
