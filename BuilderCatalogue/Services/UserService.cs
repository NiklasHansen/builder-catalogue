using BuilderCatalogue.Domain;

namespace BuilderCatalogue.Services;

internal interface IUserService
{
    Task<List<UserInfo>> GetUsers(CancellationToken cts = default);
    Task<User?> GetUserByName(string name, CancellationToken cts = default);
    Task<User?> GetUserById(string id, CancellationToken cts = default);
}

internal class UserService(HttpClient httpClient, IColorService colorService) : IUserService
{
    public async Task<List<UserInfo>> GetUsers(CancellationToken cts = default)
    {
        var response = await httpClient.GetAsync("/api/users", cts);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<UsersResponse>(cts);

        return responseBody?.Users ?? [];
    }

    public async Task<User?> GetUserByName(string name, CancellationToken cts = default)
    {
        var response = await httpClient.GetAsync($"/api/user/by-username/{name}", cts);
        response.EnsureSuccessStatusCode();

        var info = await response.Content.ReadFromJsonAsync<UserInfo>(cts);
        if (info is null)
        {
            return null;
        }

        return await GetUserById(info.Id);
    }

    public async Task<User?> GetUserById(string id, CancellationToken cts = default)
    {
        var response = await httpClient.GetAsync($"/api/user/by-id/{id}", cts);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync<UserResponse>(cts);
        if (responseBody is null)
        {
            return null;
        }

        var colors = await colorService.GetColors(cts);

        var pieces = new Dictionary<Piece, int>();
        foreach (var p in responseBody.Collection)
        {
            foreach (var v in p.Variants)
            {
                var color = colors.GetValueOrDefault(v.Color);
                pieces.Add(new Piece(p.PieceId, color ?? v.Color), v.Count);
            }
        }

        return new User
        {
            Id = responseBody.Id,
            Username = responseBody.Username,
            Location = responseBody.Location,
            BrickCount = responseBody.BrickCount,
            Pieces = pieces
        };
    }
    
    private record UsersResponse
    {
        public required List<UserInfo> Users { get; init; }
    }

    private record UserResponse : UserInfo
    {
        public required List<UserPiece> Collection { get; init; }
    }

    private record UserPiece
    {
        public required string PieceId { get; init; }
        public required List<Variant> Variants { get; init; }
    }

    private record Variant
    {
        public required string Color { get; init; }
        public required int Count { get; init; }
    }
}
