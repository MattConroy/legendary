using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Legendary.Companion;
using Legendary.Companion.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<SetupRandomizer>();
builder.Services.AddSingleton<Legendary.Companion.Data.SetCatalog>();
builder.Services.AddSingleton<Legendary.Companion.Data.KeywordCatalog>();
builder.Services.AddSingleton<GameStateService>();

await builder.Build().RunAsync();
