namespace NoCrastinator.Api.Domain
{

    public class UserResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public int TotalPoints { get; set; }
        public string? PhoneNumber { get; set; } 
    }

}
