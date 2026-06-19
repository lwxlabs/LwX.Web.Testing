# Migration from LwX.Blazor.Testing and LwX.BlazorHybrid.Testing

The universal package keeps the same feature groups and adds framework adapters.

Old:

```csharp
builder.Services.AddLwXBlazorTesting();
```

New:

```csharp
builder.Services.AddLwXWebTesting(o => o.UseBlazor());
```

Or keep the compatibility extension:

```csharp
builder.Services.AddLwXBlazorTesting();
```

Hybrid:

```csharp
builder.Services.AddLwXWebTesting(o => o.UseBlazorHybrid());
```

The old concepts are mapped as follows:

- `LwXPlaywrightOptions` -> `LwXWebTestingOptions.Playwright`
- Blazor helpers -> `LwX.Web.Testing.Extensions`
- Hybrid fake services -> universal fake service abstractions
- WebView2 helper -> `LwX.Web.Testing.WebView2.WebView2DebugHelper`
- Test host endpoints -> `MapLwXWebTestRoutes()`
