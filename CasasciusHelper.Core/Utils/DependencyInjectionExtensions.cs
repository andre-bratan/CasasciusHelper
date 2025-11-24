using B.DiskUtils;
using B.StringUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CasasciusHelper.Core.Utils;

public static class DependencyInjectionExtensions
{
    public static void RegisterUtils(this IServiceCollection services)
    {
        services.TryAddSingleton<IDirectoryAccessChecker, DirectoryAccessChecker>();
        services.TryAddSingleton<IPathUtils, PathUtils>();
        services.TryAddSingleton<IRandomStringGenerator, RandomStringGenerator>();
    }
}
