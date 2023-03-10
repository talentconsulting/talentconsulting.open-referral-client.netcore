// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceDirectoryClient.Api;
using Talentconsulting.OpenReferral.PullScript.OrganisationMappers;

namespace Talentconsulting.OpenReferral.PullScript;
class Program
{
    protected Program() { }
    public static IConfiguration Configuration { get; private set; } = default!;
    public static async Task Main(string[] args)
    {
        Configuration  = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

        var serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton<IConfiguration>(Program.Configuration)
        .AddHttpClients(Program.Configuration)
        .BuildServiceProvider();

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                .AddConsole();
        });
        ILogger logger = loggerFactory.CreateLogger<Program>();

        logger.LogDebug("Starting application");

        BuckinghamshireMapper BuckinghamshireMapper = new BuckinghamshireMapper(serviceProvider.GetRequiredService <IOrganisationClientService>());
        await BuckinghamshireMapper.AddOrUpdateServices("https://api.familyinfo.buckinghamshire.gov.uk", "api/v1");
    }
}
