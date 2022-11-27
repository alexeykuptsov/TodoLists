namespace TodoLists.App.Services;

public class UserService : IUserService
{
    public const string ProfileClaimType = "TodoListAppClaims/Profile";
    private readonly IHttpContextAccessor myHttpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        myHttpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserProfileName()
    {
        return myHttpContextAccessor.HttpContext!.User.Claims.Single(x => x.Type == ProfileClaimType).Value;
    }
}