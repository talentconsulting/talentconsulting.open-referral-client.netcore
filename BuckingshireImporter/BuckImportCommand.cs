using PluginBase;
using ServiceDirectory;
using System;
using System.Threading.Tasks;
using talentconsulting.open_referral_client;
using talentconsulting.open_referral_client.Interfaces;

namespace HelloPlugin
{
    public class BuckImportCommand : ICommand
    {
        public string Name { get => "DataImporter"; }
        public string Description { get => "Displays hello message."; }

        public async Task<int> Execute(string arg)
        {
            Console.WriteLine("Starting Buckinghamshire Mapper");
            IOpenReferralClient openReferralClient = new OpenReferralClient(new Uri("https://api.familyinfo.buckinghamshire.gov.uk"), "api/v1");
            IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


            BuckinghamshireMapper BuckinghamshireMapper = new BuckinghamshireMapper(openReferralClient, organisationClientService);
            await BuckinghamshireMapper.AddOrUpdateServices();
            return 0;
        }
    }
}
