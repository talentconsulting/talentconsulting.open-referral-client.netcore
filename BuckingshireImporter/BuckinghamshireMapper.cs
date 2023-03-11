using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Newtonsoft.Json.Linq;
using ServiceDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using talentconsulting.open_referral_client;
using talentconsulting.open_referral_client.Models;

namespace HelloPlugin;

public class BuckinghamshireMapper
{
    private Dictionary<string, OrganisationDto> _dictOrganisations;
    private readonly IOrganisationClientService _organisationClientService;

    public string Name => "Buckinghamshire Mapper";

    public BuckinghamshireMapper()
    {
        _organisationClientService = new OrganisationClientService("https://localhost:7022/");
    }

    public async Task AddOrUpdateServices(string baseurl, string basepath)
    {
        var client = new OpenReferralClient(new Uri(baseurl), basepath);

        var services = await client.GetServices(new
        {

        });

        await AddAndUpdateServices(services);

    }

    private async Task AddAndUpdateServices(ServiceResponse serviceResponse)
    {
        await CreateOrganisationDictionary();

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

            string serviceId = service.Id.ToString();

            var newService = builder.WithMainProperties(
                id: serviceId,
                serviceType: new ServiceTypeDto("1", "Information Sharing", ""),
                organisationId: organisationDto.Id,
                name: service.Name,
                description: service.Description,
                accreditations: null,
                assuredDate: null,
                attendingAccess: null,
                attendingType: null,
                deliverableType: null,
                status: "active",
                fees: null,
                canFamilyChooseDeliveryLocation: false)
                .WithEligibility(GetEligibilityDtos(service.Data))
                .WithCostOption(GetCostOptionDtos(serviceId, service.Data))
                .WithLinkContact(GetLinkContactDtos(serviceId, service.Contacts))
                .WithServiceAtLocations(GetServiceAtLocations(serviceId, service.Locations, service.RegularSchedules, service.Taxonomies))
                .Build();

            if (newOrganisation)
            {
                //Create Organisation
                OrganisationWithServicesDto organisationWithServicesDto = new OrganisationWithServicesDto(
                id: organisationDto.Id,
                organisationType: organisationDto.OrganisationType,
                name: organisationDto.Name,
                description: organisationDto.Description,
                logo: organisationDto.Logo,
                uri: organisationDto.Uri,
                url: organisationDto.Url,
                services: new List<ServiceDto>() { newService },
                linkContacts: new List<LinkContactDto>());

                //await _organisationClientService.CreateOrganisation(organisationWithServicesDto);
            }
            else
            {
                
                OrganisationWithServicesDto organisationWithServicesDto = await _organisationClientService.GetOrganisationById(organisationDto.Id);
                if (organisationWithServicesDto.Services == null)
                {
                    organisationWithServicesDto.Services = new List<ServiceDto>();
                }

                organisationWithServicesDto.Services.Add(newService);

                //await _organisationClientService.UpdateOrganisation(organisationWithServicesDto);
                
            }
        }
    }

    private async Task CreateOrganisationDictionary()
    {
        _dictOrganisations = new Dictionary<string, OrganisationDto>();
        List<OrganisationDto> organisations = await _organisationClientService.GetListOrganisations();
        foreach (var organisation in organisations)
        {
            _dictOrganisations[organisation.Id] = organisation;
        }
    }

    private List<EligibilityDto> GetEligibilityDtos(Dictionary<string, JToken> data)
    {

        int minage = 0;
        int maxage = 127;
        if (data.ContainsKey("min_age") && data.TryGetValue("min_age", out JToken? minagetoken) && minagetoken.ToString().Any())
        {
            int.TryParse(minagetoken.ToString(), out minage);
        }
        if (data.ContainsKey("max_age") && data.TryGetValue("max_age", out JToken? maxagetoken) && maxagetoken.ToString().Any())
        {
            int.TryParse(maxagetoken.ToString(), out maxage);
        }

        return new List<EligibilityDto>
        {
            new EligibilityDto(id: Guid.NewGuid().ToString(), eligibilityDescription: string.Empty, maximumAge: maxage, minimumAge: minage)
        };
    }

    private List<CostOptionDto> GetCostOptionDtos(string serviceId, Dictionary<string, JToken> data)
    {
        if (data.ContainsKey("free"))
        {
            return new List<CostOptionDto>();
        }

        string amountDescription = GetValueFromDictionary("amount_description", data);
        decimal amount = 0;
        if (data.ContainsKey("amount") && data.TryGetValue("amount", out JToken? amountToken) && amountToken.ToString().Any())
        {
            decimal.TryParse(amountToken.ToString(), out amount);
        }
        string? option = null;
        if (data.ContainsKey("option") && data.TryGetValue("option", out JToken? optionToken) && optionToken.ToString().Any())
        {
            option = optionToken.ToString();
        }
        DateTime? validFrom = null; ;
        if (data.ContainsKey("valid_from") && data.TryGetValue("valid_from", out JToken? validFromToken) && validFromToken.ToString().Any() && DateTime.TryParse(validFromToken.ToString(), out DateTime dtValidFrom))
        {
            validFrom = dtValidFrom;

        }
        DateTime? validTo = null;
        if (data.ContainsKey("valid_to") && data.TryGetValue("valid_to", out JToken? validToToken) && validToToken.ToString().Any() && DateTime.TryParse(validToToken.ToString(), out DateTime dtValidTo))
        {
            validTo = dtValidTo;
        }

        return new List<CostOptionDto>()
        {
            new CostOptionDto(id: Guid.NewGuid().ToString(), amountDescription: amountDescription, amount: amount, linkId: serviceId, option: option, validFrom: validFrom, validTo: validTo)
        };
    }

    private List<LinkContactDto> GetLinkContactDtos(string serviceId, IList<Contact> contacts)
    {
        if (!contacts.Any())
        {
            return new List<LinkContactDto>();
        }

        var list = new List<LinkContactDto>();
        foreach (var contact in contacts)
        {
            list.Add(new LinkContactDto(id: Guid.NewGuid().ToString(), linkId: serviceId, linkType: "ServiceContact", new ContactDto(id: Guid.NewGuid().ToString(), title: contact.Title, name: contact.Name, telephone: contact.Phone, textPhone: contact.Phone, url: null, email: contact.Email)));
        }

        return list;
    }

    private List<ServiceAtLocationDto> GetServiceAtLocations(string serviceId, IList<Location> locations, IList<RegularSchedules> regularSchedules, IList<Taxonomy> taxonomies)
    {
        if (!locations.Any())
        {
            return new List<ServiceAtLocationDto>();
        }

        var list = new List<ServiceAtLocationDto>();
        foreach (var location in locations)
        {
            var physicalAddresses = new List<PhysicalAddressDto>()
            {
                new PhysicalAddressDto(id: Guid.NewGuid().ToString(), address1: GetValueFromDictionary("address_1", location.Data), city: GetValueFromDictionary("city", location.Data), postCode: GetValueFromDictionary("postal_code", location.Data), country: GetValueFromDictionary("country", location.Data), stateProvince: GetValueFromDictionary("state_province", location.Data))
            };

            var listRegularSchedules = new List<RegularScheduleDto>();
            foreach (var regularSchedule in regularSchedules)
            {
                listRegularSchedules.Add(new RegularScheduleDto(id: regularSchedule.Id.ToString(), description: $"Opens at:{regularSchedule.OpensAt} Closes at: {regularSchedule.ClosesAt}", opensAt: null, closesAt: null, byDay: regularSchedule.Weekday, byMonthDay: null, dtStart: null, freq: null, interval: null, validFrom: null, validTo: null));
            }
            var listtaxonomies = new List<LinkTaxonomyDto>();
            foreach (var taxonomy in taxonomies)
            {
                listtaxonomies.Add(new LinkTaxonomyDto(id: Guid.NewGuid().ToString(), serviceId, "Location",
                    new TaxonomyDto(taxonomy.Id.ToString(), taxonomy.Name, taxonomyType: TaxonomyType.ServiceCategory, parent: null)));
            }

            var locationDto = new LocationDto(id: Guid.NewGuid().ToString(), name: location.Name, description: location.Name, latitude: location.Geometry.Coordinates[1], longitude: location.Geometry.Coordinates[1], physicalAddresses: physicalAddresses, linkTaxonomies: listtaxonomies, linkContacts: new List<LinkContactDto>());

            list.Add(new ServiceAtLocationDto(id: location.Id.ToString(), location: locationDto, regularSchedules: listRegularSchedules, holidaySchedules: new List<HolidayScheduleDto>(), linkContacts: new List<LinkContactDto>()));
        }

        return list;
    }

    private string GetValueFromDictionary(string key, Dictionary<string, JToken> data)
    {
        if (!data.ContainsKey(key))
        {
            return string.Empty;
        }

        if (data.TryGetValue(key, out JToken? token) && token.ToString().Any())
        {
            return token.ToString();
        }

        return string.Empty;
    }
}