namespace NoCrastinator.Api.Domain
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }
    }

}
