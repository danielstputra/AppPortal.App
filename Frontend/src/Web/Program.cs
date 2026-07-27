using Web.Components;
using Web.Middleware;
using Web.Models;
using Web.Services;
using Web.Services.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}
else
{
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else
            policy.WithOrigins().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", $"AppPortal/{AppVersion.Version}");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    })
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDevExpressBlazor();

builder.Services.AddScoped<IApiClient, MockApiClient>();
builder.Services.AddScoped<IEmployeeService, MockEmployeeService>();
builder.Services.AddScoped<LocalizationService>();

var app = builder.Build();

var idPath = Path.Combine(app.Environment.WebRootPath, "translations", "id.json");
var enPath = Path.Combine(app.Environment.WebRootPath, "translations", "en.json");
Translations.Initialize(idPath, enPath);

app.UseSecurityHeaders();
app.UseRequestValidation();

app.UseWhen(ctx => ctx.Request.Path == "/favicon.ico", b =>
{
    b.Run(ctx => { ctx.Response.Redirect("/favicon.png", true); return Task.CompletedTask; });
});

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("ApiPolicy");
app.UseAntiforgery();
app.MapReverseProxy();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Web.Client._Imports).Assembly);

app.Run();
