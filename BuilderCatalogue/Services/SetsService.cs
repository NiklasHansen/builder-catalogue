using BuilderCatalogue.Domain;

namespace BuilderCatalogue.Services;

internal interface ISetsService
{
    Task<List<SetInfo>> GetSets(CancellationToken cts = default);
    Task<Set?> GetSetByName(string name, CancellationToken cts = default);
    Task<Set?> GetSetById(string id, CancellationToken cts = default);
}

internal class SetsService(HttpClient httpClient, IColorService colorService) : ISetsService
{
    public async Task<List<SetInfo>> GetSets(CancellationToken cts = default)
    {
        var response = await httpClient.GetAsync("/api/sets", cts);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<SetsResponse>(cts);

        return responseBody?.Sets ?? [];
    }

    public async Task<Set?> GetSetByName(string name, CancellationToken cts = default)
    {
        var response = await httpClient.GetAsync($"/api/set/by-name/{name}", cts);
        response.EnsureSuccessStatusCode();

        var info = await response.Content.ReadFromJsonAsync<SetInfo>(cts);
        if (info is null)
        {
            return null;
        }

        return await GetSetById(info.Id);
    }

    public async Task<Set?> GetSetById(string id, CancellationToken cts = default)
    {
        var response = await httpClient.GetAsync($"/api/set/by-id/{id}", cts);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<SetResponse>(cts);
        if (responseBody is null)
        {
            return null;
        }
        
        var colors = await colorService.GetColors(cts);
        
        var pieces = new Dictionary<Piece, int>();
        foreach (var p in responseBody.Pieces)
        {
            var color = colors.GetValueOrDefault(p.Part.Material.ToString());
            pieces.Add(new Piece(p.Part.DesignId, color ?? p.Part.Material.ToString()), p.Quantity);
        }

        return new Set
        {
            Id = responseBody.Id,
            Name = responseBody.Name,
            SetNumber = responseBody.SetNumber,
            TotalPieces = responseBody.TotalPieces,
            Pieces = pieces
        };
    }
    
    private record SetsResponse
    {
        public required List<SetInfo> Sets { get; init; }
    }

    private record SetResponse : SetInfo
    {
        public required List<SetPiece> Pieces { get; init; }
    }
    
    private record SetPiece
    {
        public required SetPart Part { get; init; }
        public int Quantity { get; init; }
    }
    
    private record SetPart
    {
        public required string DesignId { get; init; }
        public int Material { get; init; }
        public required string PartType { get; init; }
    }
}