using PluginBase;
using PublicPartnershipImporter;
using PublicPartnershipImporter.Service;
using ServiceDirectory;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using talentconsulting.open_referral_client;
using talentconsulting.open_referral_client.Interfaces;

namespace HelloPlugin
{
    public class PublicPartnershipImportCommand : IDataInputCommand
    {
        public string Name { get => "DataImporter"; }
        public string Description { get => "Imports some other Data."; }

        public async Task<int> Execute(string arg)
        {
            Console.WriteLine($"Starting Public Partnership Mapper (Hull City)");
#pragma warning disable S1075 // URIs should not be hardcoded
            IPublicPartnershipClientService publicPartnershipClientService = new PublicPartnershipClientService("https://lgaapi.connecttosupport.org/");
            IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


            PublicPartnershipMapper publicPartnershipMapper = new PublicPartnershipMapper(publicPartnershipClientService, organisationClientService, "E06000010");
#pragma warning restore S1075 // URIs should not be hardcoded
            await publicPartnershipMapper.AddOrUpdateServices();
            Console.WriteLine($"Finished Public Partnership Mapper (Hull City)");
            return 0;
        }
    }
}
