using System.Collections.Concurrent;
using BuilderCatalogue.Domain;
using BuilderCatalogue.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BuilderCatalogue.Features.Collab;

public static class Collab
{
    public static WebApplication MapCollab(this WebApplication app)
    {
        app.MapGet("/user/{userName}/collab/{setName}", async Task<Results<Ok<IEnumerable<string>>, NotFound>>([FromServices] IUserService userService, [FromServices] ISetsService setsService, [FromRoute] string userName, [FromRoute] string setName, CancellationToken cts = default) =>
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

            var missingPieces = new PieceCollection();
            foreach (var (piece, quantity) in user.Pieces.MissingPieces(set.Pieces))
            {
                missingPieces.Add(piece, quantity);
            }

            if (!missingPieces.Any())
            {
                return TypedResults.Ok(Enumerable.Empty<string>());
            }

            var users = await userService.GetUsers(cts);
            users = users.Where(u => u.Id != user.Id).ToList();
    
            var collabs = new ConcurrentBag<string>();
            await Parallel.ForEachAsync(users, cts, async (u, cancellationToken) =>
            {
                var otherUser = await userService.GetUserById(u.Id, cancellationToken);
                if (otherUser is null)
                {
                    return;
                }

                if (!otherUser.Pieces.MissingPieces(missingPieces).Any())
                {
                    collabs.Add(otherUser.Username);
                }
            });

            return TypedResults.Ok((IEnumerable<string>)collabs);
        });

        return app;
    }
}