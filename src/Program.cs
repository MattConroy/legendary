using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Legendary.Companion;
using Legendary.Companion.Abstractions;
using Legendary.Companion.Data;
using Legendary.Companion.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<SetupRandomizer>();
builder.Services.AddSingleton<ISetRepository, HttpSetRepository>();
builder.Services.AddSingleton<IKeywordRepository, HttpKeywordRepository>();
builder.Services.AddSingleton<IPreferenceRepository, LocalStoragePreferenceRepository>();
builder.Services.AddSingleton<GameStateService>();

await builder.Build().RunAsync();
