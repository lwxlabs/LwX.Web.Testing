# Framework support

`LwX.Web.Testing` is framework-neutral with first-class adapter helpers for:

- Generic / plain HTML
- Static sites
- Vanilla JS
- Web Components
- Lit
- jQuery
- HTMX
- Alpine.js
- Blazor Server
- Blazor WebAssembly
- Blazor Web App
- MAUI Blazor Hybrid / WebView2 smoke tests
- Angular
- React
- Next.js
- Remix
- Vue
- Nuxt
- Svelte
- SvelteKit
- Solid
- Qwik
- Astro
- Ember

Use the matching option method when possible:

```csharp
options.UseNextJs();
options.UseNuxt();
options.UseSvelteKit();
options.UseSolid();
options.UseAstro();
options.UsePlainHtml();
```

And call the matching wait helper:

```csharp
await Page.WaitForNextJsReadyAsync();
await Page.WaitForNuxtReadyAsync();
await Page.WaitForSvelteKitReadyAsync();
await Page.WaitForSolidReadyAsync();
await Page.WaitForAstroReadyAsync();
await Page.WaitForPlainHtmlReadyAsync();
```
