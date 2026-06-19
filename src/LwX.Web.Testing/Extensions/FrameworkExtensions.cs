using Microsoft.Playwright;

namespace LwX.Web.Testing.Extensions;

public static class FrameworkExtensions
{
    public static Task WaitForAppReadyAsync(this IPage page) => page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    public static async Task WaitForGenericReadyAsync(this IPage page) { await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded); await page.WaitForFunctionAsync("() => document.readyState === 'complete'"); }
    public static Task WaitForPlainHtmlReadyAsync(this IPage page) => page.WaitForGenericReadyAsync();
    public static Task WaitForStaticSiteReadyAsync(this IPage page) => page.WaitForGenericReadyAsync();
    public static Task WaitForVanillaJsReadyAsync(this IPage page) => page.WaitForGenericReadyAsync();
    public static Task WaitForWebComponentsReadyAsync(this IPage page) => page.WaitForFunctionAsync("() => document.readyState === 'complete' && !!window.customElements");
    public static Task WaitForLitReadyAsync(this IPage page) => page.WaitForWebComponentsReadyAsync();
    public static Task WaitForJQueryReadyAsync(this IPage page) => page.WaitForFunctionAsync("() => document.readyState === 'complete' && (!window.jQuery || window.jQuery.active === 0)");
    public static Task WaitForHtmxReadyAsync(this IPage page) => page.WaitForFunctionAsync("() => document.readyState === 'complete' && (!window.htmx || !document.body.classList.contains('htmx-request'))");
    public static Task WaitForAlpineJsReadyAsync(this IPage page) => page.WaitForFunctionAsync("() => document.readyState === 'complete' && (!window.Alpine || window.Alpine.version)");

    public static async Task WaitForBlazorReadyAsync(this IPage page)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForFunctionAsync("""
            () => document.readyState === 'complete' && (!document.querySelector('#blazor-error-ui') || getComputedStyle(document.querySelector('#blazor-error-ui')).display === 'none')
        """);
    }
    public static Task WaitForBlazorNavigationAsync(this IPage page) => page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    public static Task WaitForBlazorIdleAsync(this IPage page) => page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    public static async Task WaitForNoBlazorErrorAsync(this IPage page) { var visible = await page.Locator("#blazor-error-ui:visible, .blazor-error-boundary:visible").CountAsync(); if (visible > 0) throw new InvalidOperationException("A Blazor error UI is visible."); }
    public static Task ExpectNoUnhandledBlazorErrorAsync(this IPage page) => page.WaitForNoBlazorErrorAsync();
    public static Task WaitForHybridAppReadyAsync(this IPage page) => page.WaitForBlazorReadyAsync();

    public static async Task WaitForAngularReadyAsync(this IPage page)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForFunctionAsync("""
            () => {
                const testabilities = window.getAllAngularTestabilities?.();
                if (testabilities && testabilities.length > 0) return testabilities.every(t => t.isStable());
                return document.readyState === 'complete' && !!document.querySelector('app-root, [ng-version]');
            }
        """);
    }
    public static Task WaitForAngularStableAsync(this IPage page) => page.WaitForAngularReadyAsync();
    public static Task ExpectNoAngularErrorsAsync(this IPage page) => page.ExpectNoFrameworkErrorOverlayAsync(".ng-error, pre:has-text('Error:')");
    public static Task ExpectAngularRouteAsync(this IPage page, string route) => page.ShouldHaveUrlContainingAsync(route);

    public static async Task WaitForReactReadyAsync(this IPage page)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForFunctionAsync("""
            () => {
                const root = document.querySelector('#root, #__next, [data-reactroot], [data-remix-root]');
                return !!root && root.children.length > 0;
            }
        """);
    }
    public static Task WaitForHydrationAsync(this IPage page) => page.WaitForReactReadyAsync();
    public static Task WaitForNextJsReadyAsync(this IPage page) => page.WaitForReactReadyAsync();
    public static Task WaitForRemixReadyAsync(this IPage page) => page.WaitForReactReadyAsync();
    public static Task ExpectNoReactErrorOverlayAsync(this IPage page) => page.ExpectNoFrameworkErrorOverlayAsync("vite-error-overlay, nextjs-portal, [data-nextjs-dialog-overlay]");
    public static Task ExpectNoNextJsErrorOverlayAsync(this IPage page) => page.ExpectNoReactErrorOverlayAsync();
    public static Task ExpectNoViteOverlayAsync(this IPage page) => page.ExpectNoFrameworkErrorOverlayAsync("vite-error-overlay");
    public static Task ExpectReactRouteAsync(this IPage page, string route) => page.ShouldHaveUrlContainingAsync(route);

    public static async Task WaitForVueReadyAsync(this IPage page)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForFunctionAsync("""
            () => {
                const app = document.querySelector('#app, #__nuxt, [data-v-app]');
                return !!app && app.children.length > 0;
            }
        """);
    }
    public static Task WaitForNuxtReadyAsync(this IPage page) => page.WaitForVueReadyAsync();
    public static Task ExpectNoVueErrorsAsync(this IPage page) => page.ExpectNoFrameworkErrorOverlayAsync("vite-error-overlay, .vue-error, .nuxt-error");
    public static Task ExpectVueRouteAsync(this IPage page, string route) => page.ShouldHaveUrlContainingAsync(route);
    public static Task ClearPiniaStateAsync(this IPage page) => page.EvaluateAsync("() => { localStorage.removeItem('pinia'); sessionStorage.removeItem('pinia'); }");
    public static Task SetPiniaStateAsync(this IPage page, string store, object state) => page.EvaluateAsync("([store,state]) => localStorage.setItem('pinia.' + store, JSON.stringify(state))", new object[] { store, state });

    public static Task WaitForSvelteReadyAsync(this IPage page) => page.WaitForGenericReadyAsync();
    public static Task WaitForSvelteKitReadyAsync(this IPage page) => page.WaitForSvelteReadyAsync();
    public static Task WaitForSolidReadyAsync(this IPage page) => page.WaitForGenericReadyAsync();
    public static Task WaitForQwikReadyAsync(this IPage page) => page.WaitForGenericReadyAsync();
    public static Task WaitForAstroReadyAsync(this IPage page) => page.WaitForGenericReadyAsync();
    public static Task WaitForEmberReadyAsync(this IPage page) => page.WaitForGenericReadyAsync();

    public static async Task ExpectNoFrameworkErrorOverlayAsync(this IPage page, string selector)
    {
        var count = await page.Locator(selector).CountAsync();
        if (count > 0) throw new InvalidOperationException($"Framework error overlay detected: {selector}");
    }
}
