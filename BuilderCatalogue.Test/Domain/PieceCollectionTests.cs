using BuilderCatalogue.Domain;

namespace BuilderCatalogue.Test.Domain;

public class PieceCollectionTests
{
    [Fact]
    public void MissingPieces_ReturnsEmptyIfNoMissing()
    {
        // Arrange
        var collection = new PieceCollection
        {
            { new Piece("123", "Red"), 5 }
        };
        
        var setToBuild = new PieceCollection
        {
            { new Piece("123", "Red"), 2 }
        };

        // Act
        var result = collection.MissingPieces(setToBuild);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public void MissingPieces_OnlyHasPartial()
    {
        // Arrange
        var piece = new Piece("123", "Red");
        var collection = new PieceCollection
        {
            { piece, 7 }
        };
        
        var setToBuild = new PieceCollection
        {
            { piece, 10 }
        };

        // Act
        var result = collection.MissingPieces(setToBuild).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var missing = result.First();
        Assert.Equal(piece, missing.Piece);
        Assert.Equal(3, missing.Quantity);
    }
    
    [Fact]
    public void MissingPieces_MissingPieceEntirely()
    {
        // Arrange
        var collection = new PieceCollection
        {
            { new Piece("123", "Red"), 70 }
        };
        
        var setToBuild = new PieceCollection
        {
            { new Piece("456", "Blue"), 10 }
        };

        // Act
        var result = collection.MissingPieces(setToBuild).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var missing = result.First();
        Assert.Equal(new Piece("456", "Blue"), missing.Piece);
        Assert.Equal(10, missing.Quantity);
    }
}