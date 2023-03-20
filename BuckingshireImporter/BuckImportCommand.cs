using PluginBase;
using ServiceDirectory;
using System;
using System.Threading.Tasks;
using talentconsulting.open_referral_client;
using talentconsulting.open_referral_client.Interfaces;

namespace BuckinghamshireImport
{
    public class BuckImportCommand : IDataInputCommand
    {
        public string Name { get => "DataImporter"; }
        public string Description { get => "Imports Buckinghamshire Data."; }

        public async Task<int> Execute(string arg)
        {
            Console.WriteLine("Starting Buckinghamshire Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
            IOpenReferralClient openReferralClient = new OpenReferralClient(new Uri("https://api.familyinfo.buckinghamshire.gov.uk"), "api/v1");
            IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


            BuckinghamshireMapper BuckinghamshireMapper = new BuckinghamshireMapper(openReferralClient, organisationClientService);
#pragma warning restore S1075 // URIs should not be hardcoded
            await BuckinghamshireMapper.AddOrUpdateServices();
            Console.WriteLine("Finished Buckinghamshire Mapper");
            return 0;
        }
    }
}
