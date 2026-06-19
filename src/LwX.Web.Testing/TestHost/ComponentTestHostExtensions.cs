using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LwX.Web.Testing.TestHost;

public static class ComponentTestHostExtensions
{
    public static WebApplication MapLwXComponentTestHost<TComponent>(this WebApplication app, string path) where TComponent : class
    {
        app.MapGet(path, () => Results.Content($"""
<!doctype html>
<html>
<head><title>LwX Component Test Host</title></head>
<body>
  <div id="app" data-testid="component-host">Component host placeholder for {typeof(TComponent).FullName}</div>
</body>
</html>
""", "text/html"));
        return app;
    }
}
