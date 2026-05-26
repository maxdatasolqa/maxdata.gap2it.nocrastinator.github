using Microsoft.AspNetCore.Identity;
namespace NoCrastinator.Api.Domain
{
    

    public class ApplicationUser : IdentityUser
    {
        public int TotalPoints { get; set; } = 0;
    }
}
