namespace BuilderCatalogue.Domain;

internal record User : UserInfo
{
    public required PieceCollection Pieces { get; init; }
}