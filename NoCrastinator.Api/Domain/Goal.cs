namespace NoCrastinator.Api.Domain
{
    public class Goal
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public int Points { get; set; }

        public DateTime DueDate { get; set; }

        public GoalStatus Status { get; set; } = GoalStatus.NotStarted;

        public int ProgressPercent { get; set; } = 0;

        // FK
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public bool IsEvaluated { get; set; } = false;
    }

    public enum GoalStatus
    {
        NotStarted,
        InProgress,
        Completed
    }
}
