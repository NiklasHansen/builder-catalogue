namespace BuilderCatalogue.Domain;

internal class PieceCollection : Dictionary<Piece, int>
{
    /// <summary>
    /// Returns the pieces missing from this collection, to build the specified set.
    /// </summary>
    /// <param name="setToBuild">The set to build</param>
    /// <returns>List of missing pieces</returns>
    public IEnumerable<(Piece, int)> MissingPieces(PieceCollection setToBuild)
    {
        foreach (var (piece, quantity) in setToBuild)
        {
            var userQuantity = this.GetValueOrDefault(piece);
            if (userQuantity < quantity)
            {
                yield return (piece, quantity - userQuantity);
            }
        }
    }
}