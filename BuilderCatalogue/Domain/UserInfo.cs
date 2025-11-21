namespace BuilderCatalogue.Domain;

internal record UserInfo
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public required string Location { get; init; }
    public int BrickCount { get; init; }
}