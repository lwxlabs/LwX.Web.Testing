using LwX.Web.Testing.TestHost;

var builder = WebApplication.CreateBuilder(args);
builder.Environment.EnvironmentName = "Testing";
builder.Services.AddLwXWebTesting(options => options.UseReact());

var app = builder.Build();
app.MapLwXWebTestRoutes();
app.MapGet("/", () => Results.Content("""
<!doctype html>
<html>
<body>
  <div id="root">
    <h1 data-testid="title">LwX Web Testing Sample</h1>
    <button data-testid="refresh-button" onclick="document.querySelector('#status').innerText='Updated'">Refresh</button>
    <p id="status">Ready</p>
  </div>
</body>
</html>
""", "text/html"));
app.Run();
