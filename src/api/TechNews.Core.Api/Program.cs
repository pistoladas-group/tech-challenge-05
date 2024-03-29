using System.Text.Json.Serialization;
using TechNews.Common.Library.Middlewares;
using TechNews.Core.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.ConfigureFilters())
                .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services
    .AddEndpointsApiExplorer()
    .ConfigureSwagger()
    .AddEnvironmentVariables(builder.Environment)
    .AddLoggingConfiguration(builder.Host)
    .ConfigureDatabase()
    .ConfigureDependencyInjections()
    .AddAuthConfiguration()
    .AddHealthChecks();

var app = builder.Build();

app.UseSwaggerConfiguration();
app.UseLoggingConfiguration();
app.UseHttpsRedirection();
app.UseMiddleware<ResponseHeaderMiddleware>();
app.UseAuthConfiguration();
app.MapControllers();
app.MigrateDatabase();
app.MapHealthChecks("/health");
app.Run();
