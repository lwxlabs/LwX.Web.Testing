using LwX.Web.Testing.Extensions;
using LwX.Web.Testing.Options;
using LwX.Web.Testing.Processes;
using LwX.Web.Testing.Xunit;
using Microsoft.Playwright;
using Xunit;
using static System.Formats.Asn1.AsnWriter;

public sealed class WebUiTests : XunitPlaywrightTestBase
{

    // First Build the sample app with dotnet build, then run the playwright.ps1 script to install the browsers and run the tests. 

    // Past this in PowerShell to install the browsers and run the tests:

    // cd "C:\Users\%username%\source\lwxlabs\libs\LwX.Web.Testing\samples\Sample.PlaywrightTests\bin\Debug\net8.0" or other build folder
    // Set-ExecutionPolicy -Scope Process Bypass -Force
    // .\playwright.ps1 install

    protected override void Configure(LwXWebTestingOptions options)
    {
        options.BaseUrl = "http://localhost:5055";
        options.UseReact();
        options.UseProfile(LwXTestProfile.CI);
        options.Report.Enabled = true;
        options.Steps.ScreenshotEachStep = true;
        options.AppRunner = LwXWebAppRunnerOptions.FromSolutionRelative(
            ".",
            "dotnet",
            "run --project samples/Sample.WebApp/Sample.WebApp.csproj --urls http://localhost:5055",
            "http://localhost:5055");
    }

    [Fact]
    public async Task Sample_Should_Update()
    {
        await RunSafelyAsync(async () =>
        {
            await StepAsync("Open sample app", () => GoToAsync("/"));
            await StepAsync("Click refresh", () => Page.ClickByTestIdAsync("refresh-button"));
            await StepAsync("Assert updated text", () => Page.ShouldContainTextAsync("Updated"));
        }, nameof(Sample_Should_Update));
    }
}
