using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PluginBase;

namespace AppWithPlugin
{
    class Program
    {
        protected Program() { }
        public static IConfiguration Configuration { get; private set; } = default!;
        static async Task Main(string[] args)
        {
            try
            {
                Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

                var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IConfiguration>(Program.Configuration)
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

                string[] pluginPaths = new string[]
                {
                    @"BuckingshireImporter\bin\Debug\net7.0\BuckingshireImporter.dll",
                };

                IEnumerable<ICommand> commands = pluginPaths.SelectMany(pluginPath =>
                {
                    Assembly pluginAssembly = LoadPlugin(pluginPath);
                    return CreateCommands(pluginAssembly);
                }).ToList();

                

                ICommand command = commands.FirstOrDefault(c => c.Name == "hello");
                if (command != null)
                {
                    var result = await command.Execute();

                    //Dictionary<string, OrganisationDto> dictOrganisations = new Dictionary<string, OrganisationDto>();
                    //var organisationClientService = serviceProvider.GetRequiredService<IOrganisationClientService>();
                    //List<OrganisationDto> organisations = await organisationClientService.GetListOrganisations();
                    ////foreach (var organisation in organisations)
                    ////{
                    ////    dictOrganisations[organisation.Id] = organisation;
                    ////}

                    //command.ExecuteWithDependency(organisations);

                    ////await command.ExecuteWithDependency(dictOrganisations);
                    ////List<MappedOrganisation>  mappedorganisations = await command.ExecuteWithDependency(dictOrganisations);
                }
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static Assembly LoadPlugin(string relativePath)
        {
            // Navigate up to the solution root
            string root = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(
                                Path.GetDirectoryName(typeof(Program).Assembly.Location)))))));

            string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
            Console.WriteLine($"Loading commands from: {pluginLocation}");
            PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
            return loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(pluginLocation));
        }

        static IEnumerable<ICommand> CreateCommands(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(ICommand).IsAssignableFrom(type))
                {
                    ICommand result = Activator.CreateInstance(type) as ICommand;
                    if (result != null)
                    {
                        count++;
                        yield return result;
                    }
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}
