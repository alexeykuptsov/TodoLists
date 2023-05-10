namespace TodoLists.App.Services;

public class UserService : IUserService
{
    public const string ProfileIdClaimType = "TodoListAppClaims/ProfileId";
    private readonly IHttpContextAccessor myHttpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        myHttpContextAccessor = httpContextAccessor;
    }

    public long GetCurrentUserProfileId()
    {
        var profileIdClaim = myHttpContextAccessor.HttpContext!.User.Claims.Single(x => x.Type == ProfileIdClaimType);
        return long.Parse(profileIdClaim.Value);
    }
}