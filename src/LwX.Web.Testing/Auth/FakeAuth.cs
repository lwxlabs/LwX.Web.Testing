using System.Security.Claims;

namespace LwX.Web.Testing.Auth;

public sealed class FakeAuthUser
{
    public string UserName { get; set; } = "test-user";
    public List<string> Roles { get; } = [];
    public Dictionary<string, string> Claims { get; } = new(StringComparer.OrdinalIgnoreCase);

    public ClaimsPrincipal ToPrincipal()
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, UserName) };
        claims.AddRange(Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(Claims.Select(pair => new Claim(pair.Key, pair.Value)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "LwXFakeAuth"));
    }
}

public sealed class FakeAuthStore
{
    private readonly Dictionary<string, FakeAuthUser> _users = new(StringComparer.OrdinalIgnoreCase);
    public FakeAuthUser CurrentUser { get; private set; } = new();
    public void AddUser(string userName, params string[] roles)
    {
        var user = new FakeAuthUser() { UserName = userName };
        user.Roles.AddRange(roles);
        _users[userName] = user;
    }
    public FakeAuthUser LoginAs(string userName)
    {
        if (!_users.TryGetValue(userName, out var user))
        {
            user = new() { UserName = userName };
            _users[userName] = user;
        }
        CurrentUser = user;
        return CurrentUser;
    }
    public void Clear() => CurrentUser = new();
}
