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
                    @"ElmbridgeImporter\bin\Debug\net7.0\ElmbridgeImporter.dll",
                };

                IEnumerable<ICommand> commands = pluginPaths.SelectMany(pluginPath =>
                {
                    Assembly pluginAssembly = LoadPlugin(pluginPath);
                    return CreateCommands(pluginAssembly);
                }).ToList();

                

                List<ICommand> plugins = commands.Where(c => c.Name == "DataImporter").ToList();
                if (plugins != null)
                {
                    foreach (var command in plugins) 
                    {
                        string servicedirectoryBaseUrl = Configuration["ApplicationServiceApi:ServiceDirectoryUrl"];
                        await command.Execute(servicedirectoryBaseUrl);
                    }
                    
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
            foreach (var result in from Type type in assembly.GetTypes()
                                   where typeof(ICommand).IsAssignableFrom(type)
                                   let result = Activator.CreateInstance(type) as ICommand
                                   where result != null
                                   select result)
            {
                count++;
                yield return result;
            }

            CommandCountCheck(assembly, count);
        }

        static void CommandCountCheck(Assembly assembly, int count)
        {
            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ArgumentException(
                    $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}
