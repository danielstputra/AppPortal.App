using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using DevExpress.Blazor;
using Polly;
using Web.Infrastructure.Auth;
using Web.Infrastructure.Http;
using Web.Infrastructure;
using Web.Infrastructure.Offline;
using Web.Features.Sparta.Services;
using Web.Features.Sparta.Offline;
using Web.Features.Legal.Services;
using Web.Features.Legal.Offline;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var vendorApiUrl = builder.Configuration["ApiSettings:VendorApiBaseUrl"]
    ?? throw new InvalidOperationException("Vendor API URL not found.");

// ─── HttpClient Factory ────────────────────────────────────────
builder.Services.AddHttpClient("VendorApi", client =>
{
    client.BaseAddress = new Uri(vendorApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// ─── Authenticated Client with Auth Handler ────────────────────
builder.Services.AddHttpClient("VendorApi.Authenticated", client =>
{
    client.BaseAddress = new Uri(vendorApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<AuthDelegatingHandler>();

// ─── Typed Client ──────────────────────────────────────────────
builder.Services.AddScoped<VendorHttpClient>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("VendorApi.Authenticated");
    var logger = sp.GetRequiredService<ILogger<VendorHttpClient>>();
    return new VendorHttpClient(httpClient, logger);
});

// ─── Auth ──────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddTransient<AuthDelegatingHandler>();

// ─── Offline / PWA ─────────────────────────────────────────────
builder.Services.AddScoped<NetworkStatusService>();
builder.Services.AddScoped<IndexedDbService>();
builder.Services.AddScoped<SyncQueue>();
builder.Services.AddScoped<SyncEngine>();
builder.Services.AddScoped<ConflictResolver>();
builder.Services.AddScoped<PwaService>();

// ─── Module Services ───────────────────────────────────────────
builder.Services.AddScoped<SpartaService>();
builder.Services.AddScoped<SpartaDbStore>();
builder.Services.AddScoped<SpartaSyncService>();
builder.Services.AddScoped<LegalService>();
builder.Services.AddScoped<LegalDbStore>();

// ─── Configuration ─────────────────────────────────────────────
builder.Services.AddScoped<AppConfiguration>();

// ─── Event Bus ─────────────────────────────────────────────────
builder.Services.AddSingleton<PortalEventBus>();

// ─── DevExpress ────────────────────────────────────────────────
builder.Services.AddDevExpressBlazor();

// ─── Logging ───────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Warning);

var app = builder.Build();

// ─── PWA: Register Service Worker ──────────────────────────────
var pwaService = app.Services.GetRequiredService<PwaService>();
await pwaService.RegisterServiceWorkerAsync();

// ─── IndexedDB: Initialize database ────────────────────────────
var indexedDb = app.Services.GetRequiredService<IndexedDbService>();
await indexedDb.InitializeAsync();

await app.RunAsync();
