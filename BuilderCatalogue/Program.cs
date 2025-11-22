using System.Collections.Concurrent;
using BuilderCatalogue.Domain;
using BuilderCatalogue.Options;
using BuilderCatalogue.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("ApiOptions"));

builder.Services.AddHttpClient<ISetsService, SetsService>(void (provider, client) =>
{
    var apiOptions = provider.GetRequiredService<IOptions<ApiOptions>>();
    client.BaseAddress = new Uri(apiOptions.Value.BaseUrl);
});

builder.Services.AddHttpClient<IUserService, UserService>(void (provider, client) =>
{
    var apiOptions = provider.GetRequiredService<IOptions<ApiOptions>>();
    client.BaseAddress = new Uri(apiOptions.Value.BaseUrl);
});

builder.Services.AddSingleton<IColorService, ColorService>();

var app = builder.Build();

app.MapGet("/user/{userName}/buildable-sets", async Task<Results<Ok<IEnumerable<string>>, NotFound>>([FromServices] IUserService userService, [FromServices] ISetsService setsService, [FromRoute] string userName, CancellationToken cts = default) =>
{
    var user = await userService.GetUserByName(userName, cts);
    if (user is null)
    {
        return TypedResults.NotFound();
    }
    
    var buildableSets = new ConcurrentBag<string>();

    var sets = await setsService.GetSets(); // 15
    sets = sets.Where(s => s.TotalPieces <= user.BrickCount).ToList(); // 13
    
    await Parallel.ForEachAsync(sets, async (setInfo, cancellationToken) =>
    {
        var set = await setsService.GetSetById(setInfo.Id, cancellationToken);
        if (set is null)
        {
            return;
        }

        var buildable = true;
        foreach (var (piece, quantity) in set.Pieces)
        {
            var userQuantity = user.Pieces.GetValueOrDefault(piece);
            if (userQuantity < quantity)
            {
                buildable = false;
                break;
            }
        }

        if (buildable)
        {
            buildableSets.Add(set.Name);
        }
    });
    
    return TypedResults.Ok((IEnumerable<string>)buildableSets);
});

app.MapGet("/user/{userName}/collab/{setName}", async Task<Results<Ok<List<string>>, NotFound>>([FromServices] IUserService userService, [FromServices] ISetsService setsService, [FromRoute] string userName, [FromRoute] string setName, CancellationToken cts = default) =>
{
    var user = await userService.GetUserByName(userName, cts);
    if (user is null)
    {
        return TypedResults.NotFound();
    }

    var set = await setsService.GetSetByName(setName, cts);
    if (set is null)
    {
        return TypedResults.NotFound();
    }

    var missingPieces = new Dictionary<Piece, int>();
    foreach (var (piece, quantity) in set.Pieces) // TODO: Refactor into method - yield return missing quantity. In this case, add each result to a dictionary. In previous case, break on first missing piece.
    {
        var userQuantity = user.Pieces.GetValueOrDefault(piece);
        if (userQuantity < quantity)
        {
            missingPieces.Add(piece, quantity - userQuantity);
        }
    }

    if (!missingPieces.Any())
    {
        return TypedResults.Ok(new List<string>());
    }

    var users = await userService.GetUsers(cts);
    users = users.Where(u => u.Id != user.Id).ToList();
    
    var collabs = new List<string>();
    foreach (var u in users)
    {
        var otherUser = await userService.GetUserById(u.Id, cts);
        if (otherUser is null)
        {
            continue;
        }

        var possibleCollab = true;
        foreach (var (missingPiece, quantity) in missingPieces)
        {
            var userQuantity = otherUser.Pieces.GetValueOrDefault(missingPiece);
            if (userQuantity < quantity)
            {
                possibleCollab = false;
                break;
            }
        }

        if (possibleCollab)
        {
            collabs.Add(otherUser.Username);
        }
    }

    return TypedResults.Ok(collabs);
});

app.Run();

public partial class Program {}