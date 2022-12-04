using NodaTime;

namespace TodoLists.App.Models;

public class RefreshToken
{
    public required string Token { get; set; }
    public required Instant Created { get; set; }
    public required Instant Expires { get; set; }
}