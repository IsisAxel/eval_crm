using Application.Common.Services.EmailManager;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.CsvManager;

public static class DI
{
    public static IServiceCollection RegisterCsvManager(this IServiceCollection services)
    {
        services.AddTransient<ICsvService, CsvService>();
        return services;
    }
}
