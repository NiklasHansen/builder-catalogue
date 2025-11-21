namespace BuilderCatalogue.Domain;

internal record User : UserInfo
{
    public required Dictionary<Piece, int> Pieces { get; init; }
}