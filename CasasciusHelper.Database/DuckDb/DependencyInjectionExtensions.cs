using B.DatabaseUtils;
using B.DatabaseUtils.DuckDb;
using B.DatabaseUtils.DuckDb.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CasasciusHelper.Database.DuckDb;

public static class DependencyInjectionExtensions
{
    public static void RegisterDuckDbUtils(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DuckDbSettings>(configuration.GetSection(nameof(DuckDbSettings)));

        services.TryAddSingleton<DuckDbConnectionStringProvider>();
        services.AddSingleton<IDatabaseConnectionStringProvider>(sp => sp.GetRequiredService<DuckDbConnectionStringProvider>());
        services.TryAddSingleton<IDuckDbConnectionStringProvider>(sp => sp.GetRequiredService<DuckDbConnectionStringProvider>());

        services.TryAddSingleton<IDuckDbConnectionProvider, DuckDbConnectionProvider>();

        services.TryAddSingleton<DuckDbConnectionChecker>();
        services.AddSingleton<IDatabaseConnectionChecker>(sp => sp.GetRequiredService<DuckDbConnectionChecker>());
        services.TryAddSingleton<IDuckDbConnectionChecker>(sp => sp.GetRequiredService<DuckDbConnectionChecker>());
    }
}
