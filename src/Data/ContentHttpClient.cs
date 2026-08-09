namespace Legendary.Companion.Data;

/// <summary>
/// Names the <see cref="System.Net.Http.IHttpClientFactory"/> client configured with
/// the app's base address, used by the repositories that fetch content JSON.
/// </summary>
public static class ContentHttpClient
{
    public const string Name = "content";
}
