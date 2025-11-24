using CasasciusHelper.Core.Configuration;
using CasasciusHelper.Core.Data;
using CasasciusHelper.Core.Data.Import;
using CasasciusHelper.Core.Services;
using CasasciusHelper.Core.State;
using CasasciusHelper.Core.Utils;
using CasasciusHelper.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CasasciusHelper.Core;

public static class DependencyInjectionExtensions
{
    public static void RegisterCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterUtils();

        services.AddHttpClient();

        services.Configure<ApplicationSettings>(configuration.GetSection(nameof(ApplicationSettings)));

        services.RegisterDatabase(configuration);

        services.AddSingleton<ApplicationState>();

        services.AddSingleton<ICasasciusFacade, CasasciusFacade>();

        services.AddSingleton<ICasasciusDataCache, CasasciusDataCache>();
        services.AddSingleton<ICasasciusTrackerCsvReader, CasasciusTrackerCsvReader>();
        services.AddTransient<IMiniKeyService, MiniKeyService>(); // Transient scope is not an error here - see implementation
        services.AddTransient<IMiniKeySolver, MiniKeySolver>(); // Transient scope to avoid captive dependency of MiniKeyService
        services.AddSingleton<IWifPrivateKeyService, WifPrivateKeyService>();

        services.AddTransient<IMessageSigningService, MessageSigningService>(); // Transient scope to avoid captive dependency of MiniKeyService
    }
}
