using Microsoft.Playwright;
using LwX.Web.Testing.Options;

namespace LwX.Web.Testing.Adapters;

public interface IFrontendFrameworkAdapter
{
    string Name { get; }
    Task WaitForReadyAsync(IPage page, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetFrameworkErrorsAsync(IPage page);
    Task<bool> HasFrameworkErrorOverlayAsync(IPage page);
    Task ClearFrameworkStateAsync(IPage page);
}

public abstract class FrontendFrameworkAdapterBase : IFrontendFrameworkAdapter
{
    public abstract string Name { get; }
    protected virtual string ReadyScript => "() => document.readyState === 'complete'";
    protected virtual string ErrorSelector => "vite-error-overlay, [data-nextjs-dialog-overlay], nextjs-portal, .error, [role='alert']";

    public virtual async Task WaitForReadyAsync(IPage page, CancellationToken cancellationToken = default)
    {
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await page.WaitForFunctionAsync(ReadyScript);
    }

    public virtual async Task<IReadOnlyList<string>> GetFrameworkErrorsAsync(IPage page)
    {
        var locator = page.Locator(ErrorSelector);
        var count = await locator.CountAsync();
        var result = new List<string>();
        for (var i = 0; i < count; i++)
        {
            try
            {
                var text = await locator.Nth(i).InnerTextAsync(new() { Timeout = 1000 });
                if (!string.IsNullOrWhiteSpace(text)) result.Add(text.Trim());
            }
            catch { }
        }
        return result;
    }

    public virtual async Task<bool> HasFrameworkErrorOverlayAsync(IPage page) => await page.Locator(ErrorSelector).CountAsync() > 0;
    public virtual Task ClearFrameworkStateAsync(IPage page) => Task.CompletedTask;
}

public sealed class GenericFrameworkAdapter : FrontendFrameworkAdapterBase { public override string Name => "Generic"; }
public sealed class PlainHtmlFrameworkAdapter : FrontendFrameworkAdapterBase { public override string Name => "PlainHtml"; }
public sealed class WebComponentsFrameworkAdapter : FrontendFrameworkAdapterBase { public override string Name => "WebComponents"; protected override string ReadyScript => "() => document.readyState === 'complete' && !!window.customElements"; }
public sealed class LitFrameworkAdapter : FrontendFrameworkAdapterBase { public override string Name => "Lit"; protected override string ReadyScript => "() => document.readyState === 'complete'"; }
public sealed class HtmxFrameworkAdapter : FrontendFrameworkAdapterBase { public override string Name => "HTMX"; protected override string ReadyScript => "() => document.readyState === 'complete' && (!window.htmx || !document.body.classList.contains('htmx-request'))"; }
public sealed class AlpineFrameworkAdapter : FrontendFrameworkAdapterBase { public override string Name => "AlpineJs"; protected override string ReadyScript => "() => document.readyState === 'complete' && (!window.Alpine || window.Alpine.version)"; }

public class BlazorFrameworkAdapter : FrontendFrameworkAdapterBase
{
    public override string Name => "Blazor";
    protected override string ErrorSelector => "#blazor-error-ui:visible, .blazor-error-boundary:visible, .blazor-error-ui:visible";
    protected override string ReadyScript => """
        () => document.readyState === 'complete' && (!document.querySelector('#blazor-error-ui') || getComputedStyle(document.querySelector('#blazor-error-ui')).display === 'none')
    """;
}

public sealed class BlazorHybridFrameworkAdapter : BlazorFrameworkAdapter { public override string Name => "BlazorHybrid"; }

public sealed class AngularFrameworkAdapter : FrontendFrameworkAdapterBase
{
    public override string Name => "Angular";
    protected override string ErrorSelector => ".ng-error, app-root .error, pre:has-text('Error:'), [ng-version] .error";
    protected override string ReadyScript => """
        () => {
            const testabilities = window.getAllAngularTestabilities?.();
            if (testabilities && testabilities.length > 0) return testabilities.every(t => t.isStable());
            return document.readyState === 'complete' && !!document.querySelector('app-root, [ng-version]');
        }
    """;
}

public class ReactFrameworkAdapter : FrontendFrameworkAdapterBase
{
    public override string Name => "React";
    protected override string ErrorSelector => "vite-error-overlay, nextjs-portal, [data-nextjs-dialog-overlay], iframe[src*='webpack-dev-server']";
    protected override string ReadyScript => """
        () => {
            const root = document.querySelector('#root, #__next, [data-reactroot], [data-remix-root]');
            return !!root && root.children.length > 0;
        }
    """;
}
public sealed class NextJsFrameworkAdapter : ReactFrameworkAdapter { public override string Name => "NextJs"; }
public sealed class RemixFrameworkAdapter : ReactFrameworkAdapter { public override string Name => "Remix"; }

public class VueFrameworkAdapter : FrontendFrameworkAdapterBase
{
    public override string Name => "Vue";
    protected override string ErrorSelector => "vite-error-overlay, .vue-error, .nuxt-error, #__nuxt .error";
    protected override string ReadyScript => """
        () => {
            const app = document.querySelector('#app, #__nuxt, [data-v-app]');
            return !!app && app.children.length > 0;
        }
    """;
}
public sealed class NuxtFrameworkAdapter : VueFrameworkAdapter { public override string Name => "Nuxt"; }

public class SvelteFrameworkAdapter : FrontendFrameworkAdapterBase
{
    public override string Name => "Svelte";
    protected override string ErrorSelector => "vite-error-overlay, .svelte-error, [data-sveltekit-error]";
    protected override string ReadyScript => "() => document.readyState === 'complete' && !!document.body.children.length";
}
public sealed class SvelteKitFrameworkAdapter : SvelteFrameworkAdapter { public override string Name => "SvelteKit"; }

public sealed class SolidFrameworkAdapter : FrontendFrameworkAdapterBase { public override string Name => "Solid"; protected override string ErrorSelector => "vite-error-overlay, .solid-error"; }
public sealed class QwikFrameworkAdapter : FrontendFrameworkAdapterBase { public override string Name => "Qwik"; protected override string ErrorSelector => "vite-error-overlay, .qwik-error"; }
public sealed class AstroFrameworkAdapter : FrontendFrameworkAdapterBase { public override string Name => "Astro"; protected override string ErrorSelector => "vite-error-overlay, astro-dev-toolbar, .astro-error"; }
public sealed class EmberFrameworkAdapter : FrontendFrameworkAdapterBase { public override string Name => "Ember"; protected override string ReadyScript => "() => document.readyState === 'complete'"; }

public static class FrontendFrameworkAdapterFactory
{
    public static IFrontendFrameworkAdapter Create(LwXFrontendFramework framework) => framework switch
    {
        LwXFrontendFramework.PlainHtml or LwXFrontendFramework.StaticSite or LwXFrontendFramework.VanillaJs or LwXFrontendFramework.JQuery => new PlainHtmlFrameworkAdapter(),
        LwXFrontendFramework.WebComponents => new WebComponentsFrameworkAdapter(),
        LwXFrontendFramework.Lit => new LitFrameworkAdapter(),
        LwXFrontendFramework.HTMX => new HtmxFrameworkAdapter(),
        LwXFrontendFramework.AlpineJs => new AlpineFrameworkAdapter(),
        LwXFrontendFramework.Blazor or LwXFrontendFramework.BlazorServer or LwXFrontendFramework.BlazorWebAssembly or LwXFrontendFramework.BlazorWebApp => new BlazorFrameworkAdapter(),
        LwXFrontendFramework.BlazorHybrid => new BlazorHybridFrameworkAdapter(),
        LwXFrontendFramework.Angular => new AngularFrameworkAdapter(),
        LwXFrontendFramework.React => new ReactFrameworkAdapter(),
        LwXFrontendFramework.NextJs => new NextJsFrameworkAdapter(),
        LwXFrontendFramework.Remix => new RemixFrameworkAdapter(),
        LwXFrontendFramework.Vue => new VueFrameworkAdapter(),
        LwXFrontendFramework.Nuxt => new NuxtFrameworkAdapter(),
        LwXFrontendFramework.Svelte => new SvelteFrameworkAdapter(),
        LwXFrontendFramework.SvelteKit => new SvelteKitFrameworkAdapter(),
        LwXFrontendFramework.Solid => new SolidFrameworkAdapter(),
        LwXFrontendFramework.Qwik => new QwikFrameworkAdapter(),
        LwXFrontendFramework.Astro => new AstroFrameworkAdapter(),
        LwXFrontendFramework.Ember => new EmberFrameworkAdapter(),
        _ => new GenericFrameworkAdapter()
    };
}
