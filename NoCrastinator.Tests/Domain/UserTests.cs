using NoCrastinator.Api.Domain;

namespace NoCrastinator.Tests.Domain
{
    public class UserTests
    {
        [Fact]
        public void NewUser_DefaultPoints_ShouldBeZero()
        {
            var user = new ApplicationUser();

            Assert.Equal(0, user.TotalPoints);
        }
    }
}