namespace LwX.Web.Testing.Options;

public enum LwXBrowserKind { Chromium, Firefox, WebKit }
public enum LwXFrontendFramework
{
    Generic, PlainHtml, StaticSite, VanillaJs, WebComponents, Lit, JQuery, HTMX, AlpineJs,
    Blazor, BlazorServer, BlazorWebAssembly, BlazorWebApp, BlazorHybrid,
    Angular, React, NextJs, Remix, Vue, Nuxt, Svelte, SvelteKit, Solid, Qwik, Astro, Ember
}
public enum LwXTestProfile
{
    Local, CI, Debug, Smoke, Full, Visual, Server, WebAssembly, WebApp, Hybrid, WebView2,
    Angular, React, Vue, NextJs, Nuxt, Svelte, SvelteKit, Solid, Astro, PlainHtml, StaticSite
}
public enum TestKeyboardPlatform { Auto, Windows, Linux, Mac }
public enum ViewportPreset { DesktopLarge, Desktop, Laptop, Tablet, Mobile, SmallMobile, MauiWindowSmall, MauiWindowLarge }
public enum LwXColorScheme { NoPreference, Light, Dark }
public enum LwXTestStatus { Running, Passed, Failed, Skipped }
