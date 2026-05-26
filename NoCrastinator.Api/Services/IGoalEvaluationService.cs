using NoCrastinator.Api.Domain;

namespace NoCrastinator.Api.Services
{
    public interface IGoalEvaluationService
    {
        Task<int> EvaluateAsync(Goal goal);
    }
}
