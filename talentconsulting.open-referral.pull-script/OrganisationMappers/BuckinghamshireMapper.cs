using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using IdGen;
using ServiceDirectoryClient.Api;
using System.Xml.Linq;
using System;
using talentconsulting.open_referral_client.Models;

namespace Talentconsulting.OpenReferral.PullScript.OrganisationMappers;

public class BuckinghamshireMapper
{
    private readonly ILocalOfferClientService _localOfferClientService;
    private readonly IOrganisationClientService _organisationClientService;
    private readonly Dictionary<string, OrganisationDto> _dictOrganisations = new Dictionary<string, OrganisationDto>();

    public BuckinghamshireMapper(ILocalOfferClientService localOfferClientService, IOrganisationClientService organisationClientService)
    {
        _localOfferClientService = localOfferClientService;
        _organisationClientService = organisationClientService;
    }


    public async Task AddAndUpdateServices(ServiceResponse serviceResponse)
    {
        List<OrganisationDto> organisations = await _organisationClientService.GetListOrganisations();
        foreach(var organisation in  organisations) 
        {
            _dictOrganisations[organisation.Id] = organisation;
        }


        foreach (var service in serviceResponse.Services)
        {
            OrganisationDto organisationDto = default!;

            bool newOrganisation = false;
            if (_dictOrganisations.ContainsKey(service.Organisation.Id.ToString()))
            {
                organisationDto = _dictOrganisations[service.Organisation.Id.ToString()];
            }
            else
            {
                organisationDto = new OrganisationDto(
                    id: service.Organisation.Id.ToString(),
                    organisationType: new OrganisationTypeDto("2", "VCFS", "Voluntary, Charitable, Faith Sector"),
                    name: service.Organisation.Name,
                    description: service.Organisation.Description,
                    logo: default!,
                    uri: default!,
                    url: default!);

                _dictOrganisations[organisationDto.Id] = organisationDto;

                newOrganisation = true;
            }

           

            var builder = new ServicesDtoBuilder();

            var newService = builder.WithMainProperties(service.Id.ToString(),
                new ServiceTypeDto("1", "Information Sharing", ""),
                organisationDto.Id,
                service.Name,
                service.Description,
                "accreditations",
                DateTime.Now,
                    "attending access",
                    "attending type",
                    "delivery type",
                    "active",
                    null,
                    false)
                .Build();

            if (newOrganisation)
            {
                //Create Organisation
            }

        }
    }
}
