using System.Collections.Concurrent;
using BuilderCatalogue.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BuilderCatalogue.Features.BuildableSets;

public static class BuildableSets
{
    public static WebApplication MapBuildableSets(this WebApplication app)
    {
        app.MapGet("/user/{userName}/buildable-sets", async Task<Results<Ok<IEnumerable<string>>, NotFound>>([FromServices] IUserService userService, [FromServices] ISetsService setsService, [FromRoute] string userName, CancellationToken cts = default) =>
        {
            var user = await userService.GetUserByName(userName, cts);
            if (user is null)
            {
                return TypedResults.NotFound();
            }
    
            var buildableSets = new ConcurrentBag<string>();

            var sets = await setsService.GetSets(cts);
            sets = sets.Where(s => s.TotalPieces <= user.BrickCount).ToList();
    
            await Parallel.ForEachAsync(sets, cts, async (setInfo, cancellationToken) =>
            {
                var set = await setsService.GetSetById(setInfo.Id, cancellationToken);
                if (set is null)
                {
                    return;
                }

                if (!user.Pieces.MissingPieces(set.Pieces).Any())
                {
                    buildableSets.Add(set.Name);
                }
            });
    
            return TypedResults.Ok((IEnumerable<string>)buildableSets);
        });

        return app;
    }
}