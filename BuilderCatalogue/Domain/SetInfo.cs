namespace BuilderCatalogue.Domain;

internal record SetInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string SetNumber { get; init; }
    public int TotalPieces { get; init; }
}