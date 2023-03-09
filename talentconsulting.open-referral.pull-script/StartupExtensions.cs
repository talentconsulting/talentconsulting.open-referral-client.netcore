using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceDirectoryClient.Api;

namespace Talentconsulting.OpenReferral.PullScript;

public static class StartupExtensions
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.ApplicationServiceApi));

        services.AddHttpClient<IApiService, ApiService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApplicationServiceApi:ServiceDirectoryUrl")!);
        });

        services.AddHttpClient<ILocalOfferClientService, LocalOfferClientService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApplicationServiceApi:ServiceDirectoryUrl")!);
        });

        services.AddHttpClient<IOrganisationClientService, OrganisationClientService>(client =>
        {
            client.BaseAddress = new Uri(configuration.GetValue<string>("ApplicationServiceApi:ServiceDirectoryUrl")!);
        });

        return services;
    }
}
