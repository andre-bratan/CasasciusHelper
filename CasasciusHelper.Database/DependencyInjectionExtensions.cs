using CasasciusHelper.Database.DuckDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CasasciusHelper.Database;

public static class DependencyInjectionExtensions
{
    public static void RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterDuckDbUtils(configuration);

        services.AddSingleton<IDuckDbAdoQueriesRunner, DuckDbAdoAdoQueriesRunner>();
        services.AddSingleton<IDuckDbDapperQueriesRunner, DuckDbDapperQueriesRunner>();
    }
}
