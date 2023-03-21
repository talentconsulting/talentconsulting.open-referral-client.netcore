using PluginBase;
using ServiceDirectory;
using SouthamptonImporter.Service;

namespace SouthamptonImporter
{
    public class SouthamptonImportCommand : IDataInputCommand
    {
        public string Name { get => "DataImporter"; }
        public string Description { get => "Imports some other Data."; }

        public async Task<int> Execute(string arg)
        {
            Console.WriteLine($"Starting Etch Uk (Southampton City)");
#pragma warning disable S1075 // URIs should not be hardcoded
            ISouthamptonClientService southamptonClientService = new SouthamptonClientService("https://directory.southampton.gov.uk/api/");
            IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


            SouthamptonMapper publicPartnershipMapper = new SouthamptonMapper(southamptonClientService, organisationClientService, "E10000020");
#pragma warning restore S1075 // URIs should not be hardcoded
            await publicPartnershipMapper.AddOrUpdateServices();
            Console.WriteLine($"Finished Etch Uk (Southampton City)");
            return 0;
        }
    }
}