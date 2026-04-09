using Microsoft.AspNetCore.Identity;
using Moq;
using NewsSite.Models.Entities;
using Microsoft.AspNetCore.Http;


namespace Tests.Helpers
{
    public static class IdentityMockHelper
    {
        public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }
        public static Mock<SignInManager<TUser>> MockSignInManager<TUser>(Mock<UserManager<TUser>> userManager) where TUser : class
        {
           var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<TUser>>();
            return new Mock<SignInManager<TUser>>(userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null!,
                null!,
                null!,
                null!);
        }
    }
}
