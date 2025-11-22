using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BuilderCatalogue.Test.Features;

public class BuildableSetsTests
{
    [Theory]
    [InlineData("brickfan35", 3)]
    // TODO: Add tests for no buildable sets
    public async Task BuildableSets_UserCanBuildSets(string userName, int buildableSets)
    {
        // Arrange
        var waf = new WebApplicationFactory<Program>();
        var client = waf.CreateClient();
        
        // Act
        var response = await client.GetAsync($"user/{userName}/buildable-sets");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(responseBody);
        Assert.Equal(buildableSets, responseBody.Count);
    }
    
    // TODO: Add tests for verifying _which_ sets are returned
    // TODO: Add tests for error cases (service unavailable, user not found)
    // TODO: Add test for if user has no blocks in inventory
}