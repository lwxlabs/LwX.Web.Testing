using LwX.Web.Testing.Extensions;
using LwX.Web.Testing.Options;
using LwX.Web.Testing.Playwright;

public sealed class ExampleTests : PlaywrightTestBase
{
    protected override void Configure(LwXWebTestingOptions options)
    {
        options.BaseUrl = "http://localhost:5173";
        options.UseReact();
    }

    [Fact]
    public async Task Home_Should_Load()
    {
        await GoToAsync("/");
        await Page.ShouldContainTextAsync("Home");
    }
}
