using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DevExpress.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddDevExpressBlazor();

await builder.Build().RunAsync();
