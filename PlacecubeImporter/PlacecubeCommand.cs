using ElmbridgeImporter.Services;
using PlacecubeImporter;
using PluginBase;
using ServiceDirectory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using talentconsulting.open_referral_client;
using talentconsulting.open_referral_client.Interfaces;

namespace ElmbridgeImporter
{
    public class PlacecubeCommand : ICommand
    {
        public string Name { get => "DataImporter"; }
        public string Description { get => "Imports Placecube Data."; }

        public async Task<int> Execute(string arg)
        {
            List<CommandItem> commandItems = new() 
            { 
                new CommandItem() { Name = "Pennine Lancashire", BaseUrl = "https://penninelancs.openplace.directory/o/ServiceDirectoryService/v2", AdminAreaCode = "E10000017" },
                new CommandItem() { Name = "Noth Lincs", BaseUrl = "https://northlincs.openplace.directory/o/ServiceDirectoryService/v2", AdminAreaCode = "E06000013" },
                new CommandItem() { Name = "Elmbridge", BaseUrl = "https://elmbridge.openplace.directory/o/ServiceDirectoryService/v2", AdminAreaCode = "E10000030" }
            };

            foreach (var commandItem in commandItems) 
            {
                Console.WriteLine($"Starting {commandItem.Name} Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
                IPlacecubeClientService placecubeClientService = new PlacecubeClientService(commandItem.BaseUrl);
                IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


                PlacecubeMapper placecubeMapper = new PlacecubeMapper(placecubeClientService, organisationClientService, commandItem.AdminAreaCode);
#pragma warning restore S1075 // URIs should not be hardcoded
                await placecubeMapper.AddOrUpdateServices();
                Console.WriteLine($"Finished {commandItem.Name} Mapper");
            }


            //            Console.WriteLine("Starting Pennine Lancashire Mapper");
            //#pragma warning disable S1075 // URIs should not be hardcoded
            //            IPlacecubeClientService elmbridgeClientService = new PlacecubeClientService("https://penninelancs.openplace.directory/o/ServiceDirectoryService/v2");
            //            IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


            //            PlacecubeMapper elmbridgeMapper = new PlacecubeMapper(elmbridgeClientService, organisationClientService, "E10000017");
            //#pragma warning restore S1075 // URIs should not be hardcoded
            //            await elmbridgeMapper.AddOrUpdateServices();
            //            Console.WriteLine("Finished Pennine Lancashire Mapper");

            //            Console.WriteLine("Starting Noth Lincs Mapper");
            //#pragma warning disable S1075 // URIs should not be hardcoded
            //            elmbridgeClientService = new ElmbridgeClientService("https://northlincs.openplace.directory/o/ServiceDirectoryService/v2");
            //            organisationClientService = new OrganisationClientService(arg);


            //            elmbridgeMapper = new ElmbridgeMapper(elmbridgeClientService, organisationClientService, "E06000013");
            //#pragma warning restore S1075 // URIs should not be hardcoded
            //            await elmbridgeMapper.AddOrUpdateServices();
            //            Console.WriteLine("Finished Noth Lincs Mapper");


            //            Console.WriteLine("Starting Elmbridge Mapper");
            //#pragma warning disable S1075 // URIs should not be hardcoded
            //            elmbridgeClientService = new ElmbridgeClientService("https://elmbridge.openplace.directory/o/ServiceDirectoryService/v2");
            //            organisationClientService = new OrganisationClientService(arg);


            //            elmbridgeMapper = new ElmbridgeMapper(elmbridgeClientService, organisationClientService, "E10000030");
            //#pragma warning restore S1075 // URIs should not be hardcoded
            //            await elmbridgeMapper.AddOrUpdateServices();
            //            Console.WriteLine("Finished Elmbridge Mapper");
            return 0;
        }
    }
}
