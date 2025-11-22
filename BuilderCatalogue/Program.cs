using BuilderCatalogue.Features.BuildableSets;
using BuilderCatalogue.Features.Collab;
using BuilderCatalogue.Options;
using BuilderCatalogue.Services;
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

app .MapBuildableSets()
    .MapCollab();

app.Run();

public partial class Program {}