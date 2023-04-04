using ElmbridgeImporter.Services;
using FamilyHubs.ServiceDirectory.Shared.Dto;
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
    public class PlacecubeCommand : IDataInputCommand
    {
        public string Name { get => "DataImporter"; }
        public string Description { get => "Imports Placecube Data."; }

        public async Task<int> Execute(string arg)
        {
            var elmbridgeCouncil = new OrganisationWithServicesDto(
        "ddafc1ea-089c-40ba-9b41-b1a8739fb628",
            new OrganisationTypeDto("1", "LA", "Local Authority"), "Elmbridge Council", "Elmbridge Council", null, new Uri("https://www.elmbridge.gov.uk/").ToString(), "https://www.elmbridge.gov.uk/", new List<ServiceDto>(), new List<LinkContactDto>());
            elmbridgeCouncil.AdminAreaCode = "E10000030";

            var northLincCouncil = new OrganisationWithServicesDto(
        "bd1c4bc9-c513-4f88-8ff2-5b6ce2ea29e5",
            new OrganisationTypeDto("1", "LA", "Local Authority"), "North Lincolnshire Council", "North Lincolnshire Council", null, new Uri("https://www.northlincs.gov.uk/").ToString(), "https://www.northlincs.gov.uk/", new List<ServiceDto>(), new List<LinkContactDto>());
            elmbridgeCouncil.AdminAreaCode = "E06000013";

            var pennineLancashire = new OrganisationWithServicesDto(
        "342f97b2-ff2c-4a20-ba59-85aca5dc9f0a",
            new OrganisationTypeDto("1", "LA", "Local Authority"), "Pennine Lancashire", "Pennine Lancashire", null, new Uri("https://healthierpenninelancashire.co.uk/").ToString(), "https://healthierpenninelancashire.co.uk/", new List<ServiceDto>(), new List<LinkContactDto>());
            elmbridgeCouncil.AdminAreaCode = "E10000017";


            var bristolCouncil = new OrganisationWithServicesDto(
        "72e653e8-1d05-4821-84e9-9177571a6013",
            new OrganisationTypeDto("1", "LA", "Local Authority"), "Bristol County Council", "Bristol County Council", null, new Uri("https://www.bristol.gov.uk/").ToString(), "https://www.bristol.gov.uk/", new List<ServiceDto>(), new List<LinkContactDto>());
            bristolCouncil.AdminAreaCode = "E06000023";



            List<CommandItem> commandItems = new() 
            { 
                new CommandItem() { Name = "Pennine Lancashire", BaseUrl = "https://penninelancs.openplace.directory/o/ServiceDirectoryService/v2", AdminAreaCode = "E10000017", ParentOrganisation = pennineLancashire },
                new CommandItem() { Name = "North Lincolnshire Council", BaseUrl = "https://northlincs.openplace.directory/o/ServiceDirectoryService/v2", AdminAreaCode = "E06000013", ParentOrganisation = northLincCouncil },
                new CommandItem() { Name = "Elmbridge Council", BaseUrl = "https://elmbridge.openplace.directory/o/ServiceDirectoryService/v2", AdminAreaCode = "E10000030", ParentOrganisation = elmbridgeCouncil },
                new CommandItem() { Name = bristolCouncil.Name, BaseUrl = "https://bristol.openplace.directory/o/ServiceDirectoryService/v2/", AdminAreaCode = bristolCouncil.AdminAreaCode, ParentOrganisation = bristolCouncil }
            };

            foreach (var commandItem in commandItems) 
            {
                Console.WriteLine($"Starting {commandItem.Name} Mapper");
#pragma warning disable S1075 // URIs should not be hardcoded
                IPlacecubeClientService placecubeClientService = new PlacecubeClientService(commandItem.BaseUrl);
                IOrganisationClientService organisationClientService = new OrganisationClientService(arg);


                PlacecubeMapper placecubeMapper = new PlacecubeMapper(placecubeClientService, organisationClientService, commandItem.AdminAreaCode, commandItem.Name, commandItem.ParentOrganisation);
#pragma warning restore S1075 // URIs should not be hardcoded
                await placecubeMapper.AddOrUpdateServices();
                Console.WriteLine($"Finished {commandItem.Name} Mapper");
            }

            return 0;
        }
    }
}
