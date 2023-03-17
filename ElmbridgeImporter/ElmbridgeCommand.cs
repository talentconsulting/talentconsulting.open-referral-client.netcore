using ElmbridgeImporter.Services;
using PluginBase;
using ServiceDirectory;
using System;
using System.Threading.Tasks;
using talentconsulting.open_referral_client;
using talentconsulting.open_referral_client.Interfaces;

namespace ElmbridgeImporter
{
    public class ElmbridgeCommand : ICommand
    {
        public string Name { get => "DataImporter"; }
        public string Description { get => "Imports Placecube Data."; }

        public async Task<int> Execute(string arg)
        {
            Console.WriteLine("Starting Pennine Lancashire Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
            IElmbridgeClientService elmbridgeClientService = new ElmbridgeClientService("https://penninelancs.openplace.directory/o/ServiceDirectoryService/v2");
            IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


            ElmbridgeMapper elmbridgeMapper = new ElmbridgeMapper(elmbridgeClientService, organisationClientService, "E10000017");
#pragma warning restore S1075 // URIs should not be hardcoded
            await elmbridgeMapper.AddOrUpdateServices();
            Console.WriteLine("Finished Pennine Lancashire Mapper");

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
