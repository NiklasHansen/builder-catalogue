namespace BuilderCatalogue.Domain;

internal record Set : SetInfo
{
    public required PieceCollection Pieces { get; init; }
}