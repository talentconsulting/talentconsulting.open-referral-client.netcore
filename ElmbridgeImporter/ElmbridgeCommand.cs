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
        public string Description { get => "Imports Elmbridge Data."; }

        public async Task<int> Execute(string arg)
        {
            Console.WriteLine("Starting Elmbridge Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
            IOpenReferralClient openReferralClient = new OpenReferralClient(new Uri("https://elmbridge.openplace.directory/o/ServiceDirectoryService/"), "v2");
            IOpenReferralClient openReferralByServiceIdClient = new OpenReferralClient(new Uri("https://elmbridge.openplace.directory/o/ServiceDirectoryService/"), "v2");
            IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


            ElmbridgeMapper elmbridgeMapper = new ElmbridgeMapper(openReferralClient, openReferralByServiceIdClient, organisationClientService);
#pragma warning restore S1075 // URIs should not be hardcoded
            await elmbridgeMapper.AddOrUpdateServices();
            Console.WriteLine("Finished Elmbridge Mapper");
            return 0;
        }
    }
}
