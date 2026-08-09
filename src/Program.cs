using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Legendary.Companion;
using Legendary.Companion.Abstractions;
using Legendary.Companion.Data;
using Legendary.Companion.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Repositories fetch content JSON through IHttpClientFactory (a singleton), so the
// singleton repositories no longer capture a scoped HttpClient.
builder.Services.AddHttpClient(ContentHttpClient.Name,
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddSingleton<SetupRandomizer>();
builder.Services.AddSingleton<ISetRepository, HttpSetRepository>();
builder.Services.AddSingleton<IKeywordRepository, HttpKeywordRepository>();
builder.Services.AddSingleton<ICardDetailRepository, HttpCardDetailRepository>();
builder.Services.AddSingleton<IPreferenceRepository, LocalStoragePreferenceRepository>();
builder.Services.AddSingleton<GameStateService>();

await builder.Build().RunAsync();
