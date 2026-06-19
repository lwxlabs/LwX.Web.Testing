using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using LwX.Web.Testing.Extensions;
using LwX.Web.Testing.Options;
using LwX.Web.Testing.TestHost;

namespace LwX.Blazor.Testing
{
    public sealed class LwXBlazorTestingOptions : LwXWebTestingOptions
    {
        public LwXBlazorTestingOptions() => UseBlazor();
    }

    public static class BlazorTestingServiceCollectionExtensions
    {
        public static IServiceCollection AddLwXBlazorTesting(this IServiceCollection services, Action<LwXWebTestingOptions>? configure = null)
            => LwXWebTestingExtensions.AddLwXBlazorTesting(services, configure);
    }
}

namespace LwX.Blazor.Testing.Extensions
{
    public static class BlazorCompatibilityExtensions
    {
        public static Task WaitForBlazorReadyAsync(this IPage page) => FrameworkExtensions.WaitForBlazorReadyAsync(page);
        public static Task WaitForBlazorNavigationAsync(this IPage page) => FrameworkExtensions.WaitForBlazorNavigationAsync(page);
        public static Task WaitForBlazorIdleAsync(this IPage page) => FrameworkExtensions.WaitForBlazorIdleAsync(page);
        public static Task WaitForNoBlazorErrorAsync(this IPage page) => FrameworkExtensions.WaitForNoBlazorErrorAsync(page);
        public static Task ExpectNoUnhandledBlazorErrorAsync(this IPage page) => FrameworkExtensions.ExpectNoUnhandledBlazorErrorAsync(page);
    }
}
