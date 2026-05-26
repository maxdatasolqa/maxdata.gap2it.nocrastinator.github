namespace NoCrastinator.Api.Services
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }

}
