using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BuilderCatalogue.Test.Features;

public class CollabTests
{
    [Theory]
    [InlineData("landscape-artist", "tropical-island", 1)]
    // TODO: Add test for 0 collabs
    // TODO: Add test for multiple possible collabs
    public async Task Collab(string userName, string setName, int amountOfPossibleCollabs)
    {
        // Arrange
        var waf = new WebApplicationFactory<Program>();
        var client = waf.CreateClient();
        
        // Act
        var response = await client.GetAsync($"user/{userName}/collab/{setName}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(responseBody);
        Assert.Equal(amountOfPossibleCollabs, responseBody.Count);
    }
    
    [Theory]
    [InlineData("user-not-found", "tropical-island")]
    [InlineData("landscape-artist", "set-not-found")]
    public async Task Collab_NotFound(string userName, string setName)
    {
        // Arrange
        var waf = new WebApplicationFactory<Program>();
        var client = waf.CreateClient();
        
        // Act
        var response = await client.GetAsync($"user/{userName}/collab/{setName}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    // TODO: Add test for verifying _which_ users are returned
    // TODO: Tests for error cases (services unavailable, etc)
}