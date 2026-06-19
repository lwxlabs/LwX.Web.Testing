using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using LwX.Web.Testing.Extensions;
using LwX.Web.Testing.Options;
using LwX.Web.Testing.TestHost;

namespace LwX.BlazorHybrid.Testing
{
    public sealed class LwXBlazorHybridTestingOptions : LwXWebTestingOptions
    {
        public LwXBlazorHybridTestingOptions() => UseBlazorHybrid();
    }

    public static class BlazorHybridTestingServiceCollectionExtensions
    {
        public static IServiceCollection AddLwXBlazorHybridTesting(this IServiceCollection services, Action<LwXWebTestingOptions>? configure = null)
            => LwXWebTestingExtensions.AddLwXBlazorHybridTesting(services, configure);
    }
}

namespace LwX.BlazorHybrid.Testing.Extensions
{
    public static class HybridCompatibilityExtensions
    {
        public static Task WaitForBlazorReadyAsync(this IPage page) => FrameworkExtensions.WaitForBlazorReadyAsync(page);
        public static Task WaitForHybridAppReadyAsync(this IPage page) => FrameworkExtensions.WaitForHybridAppReadyAsync(page);
    }
}
