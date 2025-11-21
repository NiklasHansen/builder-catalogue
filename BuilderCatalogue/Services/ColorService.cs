using BuilderCatalogue.Options;
using Microsoft.Extensions.Options;

namespace BuilderCatalogue.Services;

internal interface IColorService
{
    Task<Dictionary<string, string>> GetColors(CancellationToken cts = default);
}

internal class ColorService(IHttpClientFactory factory, IOptions<ApiOptions> apiOptions) : IColorService
{
    private Dictionary<string, string>? _colors;
    
    public async Task<Dictionary<string, string>> GetColors(CancellationToken cts = default)
    {
        if (_colors is not null)
        {
            return _colors;
        }

        var httpClient = factory.CreateClient();
        var response = await httpClient.GetAsync($"{apiOptions.Value.BaseUrl}/api/colours", cts);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<ColorsResponse>(cancellationToken: cts);
        if (responseBody is null)
        {
            return [];
        }

        _colors = responseBody.Colours.ToDictionary(c => c.Code.ToString(), c => c.Name);
        return _colors;
    }

    private class ColorsResponse
    {
        public required List<Color> Colours { get; init; }
    }

    private class Color
    {
        public required string Name { get; init; }
        public int Code { get; init; }
    }
}