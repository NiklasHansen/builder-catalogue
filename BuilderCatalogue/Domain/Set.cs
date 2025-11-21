namespace BuilderCatalogue.Domain;

internal record Set : SetInfo
{
    public required Dictionary<Piece, int> Pieces { get; init; }
}