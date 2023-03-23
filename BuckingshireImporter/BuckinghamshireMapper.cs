using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using IdGen;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;
using PluginBase;
using ServiceDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using talentconsulting.open_referral_client.Interfaces;
using talentconsulting.open_referral_client.Models;
using static System.Net.Mime.MediaTypeNames;

namespace BuckinghamshireImport;

public class BuckinghamshireMapper
{
    private Dictionary<string, OrganisationWithServicesDto> _dictOrganisations;
    private Dictionary<string, TaxonomyDto> _dictTaxonomies;
    private List<TaxonomyDto> _extraTaxonomies;
    private readonly HashSet<string> _linkTaxIds;
    private readonly IOpenReferralClient _openReferralClient;
    private readonly IOrganisationClientService _organisationClientService;
    private const string _adminAreaCode = "E06000060";
    private const int _maxRetry = 3;
 
    public string Name => "Buckinghamshire Mapper";

    public BuckinghamshireMapper(IOpenReferralClient openReferralClient, IOrganisationClientService organisationClientService)
    {
        _openReferralClient = openReferralClient;
        _organisationClientService = organisationClientService;
        _linkTaxIds = new HashSet<string>();
        _extraTaxonomies = new List<TaxonomyDto>();
    }

    public async Task AddOrUpdateServices()
    {
        const int startPage = 1;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        var services = await _openReferralClient.GetPageServices(startPage);
        int totalPages = services.TotalPages;
        int errors = await AddAndUpdateServices(services);
        Console.WriteLine($"Completed Page {startPage} {totalPages} with {errors} errors");
        for (int i = startPage + 1; i <= totalPages;  i++) 
        {
            int retry = 0;
            while(retry < _maxRetry) 
            {
                try
                {
                    services = await _openReferralClient.GetPageServices(i);
                    retry = _maxRetry;
                }
                catch
                {
                    System.Threading.Thread.Sleep(1000);
                    retry++;
                    if (retry > _maxRetry)
                    {
                        Console.WriteLine($"Failed to get page");

                        Console.WriteLine("Extra Taxonomies:");
                        foreach (var taxonomy in _extraTaxonomies)
                        {
                            Console.WriteLine(taxonomy.Name);
                        }

                        return;

                    }
                    Console.WriteLine($"Doing retry: {retry}");
                }
            }
            
            
            errors = await AddAndUpdateServices(services);

            Console.WriteLine($"Completed Page {i} of {totalPages} with {errors} errors");
        }


        string filepath = $@"{Helper.AssemblyDirectory}\Bucks-ExtraTaxonomies.txt";
        if (File.Exists(filepath))
            File.Delete(filepath);
        using (var file = File.CreateText(filepath))
        {
            foreach (var taxonomy in _extraTaxonomies)
            {
                file.WriteLine(taxonomy.Name);
            }
        }
    }

    private async Task<int> AddAndUpdateServices(ServiceResponse serviceResponse)
    {
        List<string> errors = new List<string>();
        
        foreach (var service in serviceResponse.Services)
        {
            string serviceId = $"{_adminAreaCode.Replace("E", "")}{service.Id.ToString()}";
            OrganisationWithServicesDto serviceDirectoryOrganisation = default!;

            bool newOrganisation = false;
            string organisationId = $"{_adminAreaCode.Replace("E", "")}{service.Organisation.Id.ToString()}";
            if (_dictOrganisations.ContainsKey(organisationId))
            {
                serviceDirectoryOrganisation = _dictOrganisations[organisationId];
                //Get latest
                serviceDirectoryOrganisation = await _organisationClientService.GetOrganisationById(serviceDirectoryOrganisation.Id);
            }
            else
            {
                serviceDirectoryOrganisation = new OrganisationWithServicesDto(
                id: organisationId,
                organisationType: new OrganisationTypeDto("2", "VCFS", "Voluntary, Charitable, Faith Sector"),
                name: service.Organisation.Name,
                description: service.Organisation.Description,
                logo: default!,
                uri: default!,
                url: default!,
                services: new List<ServiceDto>(),
                linkContacts: new List<LinkContactDto>());
                serviceDirectoryOrganisation.AdminAreaCode = _adminAreaCode;

                _dictOrganisations[organisationId] = serviceDirectoryOrganisation;

                newOrganisation = true;
            }

            var builder = new ServicesDtoBuilder();

            ServiceDto existingService = serviceDirectoryOrganisation.Services.FirstOrDefault(s => s.Id == serviceId);

            var newService = builder.WithMainProperties(
                id: serviceId,
                serviceType: new ServiceTypeDto("1", "Information Sharing", ""),
                organisationId: serviceDirectoryOrganisation.Id,
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
                .WithEligibility(GetEligibilityDtos(service.Data, existingService))
                .WithCostOption(GetCostOptionDtos(serviceId, service.Data, existingService))
                .WithLinkContact(GetLinkContactDtos(serviceId, service.Contacts, existingService))
                .WithServiceAtLocations(GetServiceAtLocations(serviceId, service.Locations, service.RegularSchedules, service.Taxonomies, existingService))
                .Build();

            if (newOrganisation)
            {
                //Create Organisation
                serviceDirectoryOrganisation.Services.Add(newService);

                try
                {
                    await _organisationClientService.CreateOrganisation(serviceDirectoryOrganisation);
                }
                catch(Exception ex) 
                {
                    errors.Add($"Failed to Create Organisation with Service Id:{serviceId} {ex.Message}");
                }
                
            }
            else
            {
                
                OrganisationWithServicesDto organisationWithServicesDto = await _organisationClientService.GetOrganisationById(serviceDirectoryOrganisation.Id);
                if (organisationWithServicesDto.Services == null)
                {
                    organisationWithServicesDto.Services = new List<ServiceDto>();
                }

                organisationWithServicesDto.Services = organisationWithServicesDto.Services.Where(x => x.Id != newService.Id).ToList();
                organisationWithServicesDto.Services.Add(newService);

                try
                {
                    await _organisationClientService.UpdateOrganisation(organisationWithServicesDto);
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to Update Organisation with Service Id:{serviceId} {ex.Message}");
                }
            }
        }

        foreach(string error in errors) 
        {
            Console.WriteLine(error);
        }

        return errors.Count;
    }

    private async Task<OrganisationWithServicesDto> InitialiseBuckingshireCountyCouncil()
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        var buckCouncil = new OrganisationWithServicesDto(
        "300df704-c284-4c21-8a54-ae95d1a5d942",
            new OrganisationTypeDto("1", "LA", "Local Authority"), "Buckingshire Council", "Buckingshire Council", null, new Uri("https://www.buckinghamshire.gov.uk/").ToString(), "https://www.buckinghamshire.gov.uk/", new List<ServiceDto>(), new List<LinkContactDto>());

        buckCouncil.AdminAreaCode = _adminAreaCode;

        await _organisationClientService.CreateOrganisation(buckCouncil);
        
        return buckCouncil;
#pragma warning restore S1075 // URIs should not be hardcoded
    }

    private async Task CreateOrganisationDictionary()
    {
        _dictOrganisations = new Dictionary<string, OrganisationWithServicesDto>();
        List<OrganisationDto> organisations = await _organisationClientService.GetListOrganisations();
        var buckinghamshire = organisations.FirstOrDefault(x => x.Name.Contains("Buck"));
        if (buckinghamshire == null) 
        {
            var buckCouncil = await InitialiseBuckingshireCountyCouncil();
            _dictOrganisations[buckCouncil.Id] = buckCouncil;
        }
        
        foreach (var organisation in organisations)
        {
            OrganisationWithServicesDto organisationWithServicesDto = new OrganisationWithServicesDto(
                id: organisation.Id,
                organisationType: organisation.OrganisationType,
                name: organisation.Name,
                description: organisation.Description,
                logo: organisation.Logo,
                uri: organisation.Uri,
                url: organisation.Url,
                services: new List<ServiceDto>(),
                linkContacts: new List<LinkContactDto>());

            organisationWithServicesDto.AdminAreaCode = _adminAreaCode;

            _dictOrganisations[organisation.Id] = organisationWithServicesDto;
        }
    }

    private async Task CreateTaxonomyDictionary()
    {
        _dictTaxonomies = new Dictionary<string, TaxonomyDto>();
        var allTaxonomies = await _organisationClientService.GetTaxonomyList(1, 99);
        foreach(var taxonomy in allTaxonomies.Items)
        {
            _dictTaxonomies[taxonomy.Id] = taxonomy;
        }
    }

    private List<EligibilityDto> GetEligibilityDtos(Dictionary<string, JToken> data, ServiceDto existingService)
    {
        int id = 0;
        int minage = 0;
        int maxage = 127;
        if (data.ContainsKey("id") && data.TryGetValue("id", out JToken idtoken) && idtoken.ToString().Any())
        {
            int.TryParse(idtoken.ToString(), out id);
        }
        if (data.ContainsKey("min_age") && data.TryGetValue("min_age", out JToken minagetoken) && minagetoken.ToString().Any())
        {
            int.TryParse(minagetoken.ToString(), out minage);
        }
        if (data.ContainsKey("max_age") && data.TryGetValue("max_age", out JToken maxagetoken) && maxagetoken.ToString().Any())
        {
            int.TryParse(maxagetoken.ToString(), out maxage);
        }

        string eligibilityId = $"{_adminAreaCode.Replace("E", "")}{id.ToString()}";


        if (existingService != null)
        {
            EligibilityDto eligibilityDto = existingService.Eligibilities.FirstOrDefault(x => x.Id == eligibilityId);
            if (eligibilityDto != null) 
            {
                existingService.Eligibilities.Remove(eligibilityDto);
                existingService.Eligibilities.Add(new EligibilityDto(id: eligibilityId, eligibilityDescription: string.Empty, maximumAge: maxage, minimumAge: minage));
                return existingService.Eligibilities.ToList();
            }
        }

        return new List<EligibilityDto>
        {
            new EligibilityDto(id: eligibilityId, eligibilityDescription: string.Empty, maximumAge: maxage, minimumAge: minage)
        };
    }

    private List<CostOptionDto> GetCostOptionDtos(string serviceId, Dictionary<string, JToken> data, ServiceDto existingService)
    {
        if (data.ContainsKey("free"))
        {
            return new List<CostOptionDto>();
        }

        int id = 0;
        if (data.ContainsKey("id") && data.TryGetValue("id", out JToken idtoken) && idtoken.ToString().Any())
        {
            int.TryParse(idtoken.ToString(), out id);
        }

        string costOptionId = $"{_adminAreaCode.Replace("E", "")}{id.ToString()}";

        string amountDescription = GetValueFromDictionary("amount_description", data);
        decimal amount = 0;
        if (data.ContainsKey("amount") && data.TryGetValue("amount", out JToken amountToken) && amountToken.ToString().Any())
        {
            decimal.TryParse(amountToken.ToString(), out amount);
        }
        string option = null;
        if (data.ContainsKey("option") && data.TryGetValue("option", out JToken optionToken) && optionToken.ToString().Any())
        {
            option = optionToken.ToString();
        }
        DateTime? validFrom = null;
        if (data.ContainsKey("valid_from") && data.TryGetValue("valid_from", out JToken validFromToken) && validFromToken.ToString().Any() && DateTime.TryParse(validFromToken.ToString(), out DateTime dtValidFrom))
        {
            validFrom = dtValidFrom;
        }
        DateTime? validTo = null;
        if (data.ContainsKey("valid_to") && data.TryGetValue("valid_to", out JToken validToToken) && validToToken.ToString().Any() && DateTime.TryParse(validToToken.ToString(), out DateTime dtValidTo))
        {
            validTo = dtValidTo;
        }

        if (existingService != null)
        {
            CostOptionDto costOptionDto = existingService.CostOptions.FirstOrDefault(x => x.Id == costOptionId);
            if (costOptionDto != null)
            {
                existingService.CostOptions.Remove(costOptionDto);
                existingService.CostOptions.Add(new CostOptionDto(id: costOptionId, amountDescription: amountDescription, amount: amount, linkId: serviceId, option: option, validFrom: validFrom, validTo: validTo));
                return existingService.CostOptions.ToList();
            }
        }

        return new List<CostOptionDto>()
        {
            new CostOptionDto(id: costOptionId, amountDescription: amountDescription, amount: amount, linkId: serviceId, option: option, validFrom: validFrom, validTo: validTo)
        };
    }

    private List<LinkContactDto> GetLinkContactDtos(string serviceId, IList<Contact> contacts, ServiceDto existingService)
    {
        if (!contacts.Any())
        {
            return new List<LinkContactDto>();
        }

        var list = new List<LinkContactDto>();
        if (existingService != null) 
        {
            list = existingService.LinkContacts.ToList();
        }
        foreach (var contact in contacts)
        {
            if (string.IsNullOrEmpty(contact.Phone))
            {
                continue;
            }
            string contactId = $"{_adminAreaCode.Replace("E", "")}{contact.Id.ToString().ToString()}";
            if (existingService != null)
            {
                var linkContact = existingService.LinkContacts.FirstOrDefault(x => x.Id == contactId);
                if (linkContact != null)
                {
                    //Will be replace by the one below
                    existingService.LinkContacts.Remove(linkContact);
                }
            }
            
            list.Add(new LinkContactDto(id: contactId, linkId: serviceId, linkType: "ServiceContact", new ContactDto(id: contactId, title: contact.Title, name: contact.Name ?? "Contact", telephone: contact.Phone, textPhone: contact.Phone, url: null, email: contact.Email)));
        }

        return list;
    }

    private List<ServiceAtLocationDto> GetServiceAtLocations(string serviceId, IList<Location> locations, IList<RegularSchedules> regularSchedules, IList<Taxonomy> taxonomies, ServiceDto existingService)
    {
        if (!locations.Any())
        {
            return new List<ServiceAtLocationDto>();
        }

        var list = new List<ServiceAtLocationDto>();
        if (existingService != null) 
        {
            list = existingService.ServiceAtLocations.ToList();
        }

        foreach (var location in locations)
        {
            string locationId = $"{_adminAreaCode.Replace("E", "")}{location.Id.ToString().ToString()}";
           
            var physicalAddresses = new List<PhysicalAddressDto>()
            {
                new PhysicalAddressDto(id: locationId, address1: GetValueFromDictionary("address_1", location.Data), city: GetValueFromDictionary("city", location.Data), postCode: GetValueFromDictionary("postal_code", location.Data), country: GetValueFromDictionary("country", location.Data), stateProvince: GetValueFromDictionary("state_province", location.Data))
            };

            var listRegularSchedules = new List<RegularScheduleDto>();
            foreach (var regularSchedule in regularSchedules)
            {
                string regularScheduleId = $"{_adminAreaCode.Replace("E", "")}{regularSchedule.Id.ToString().ToString()}";
                listRegularSchedules.Add(new RegularScheduleDto(id: regularScheduleId, description: $"Opens at:{regularSchedule.OpensAt} Closes at: {regularSchedule.ClosesAt}", opensAt: null, closesAt: null, byDay: regularSchedule.Weekday, byMonthDay: null, dtStart: null, freq: null, interval: null, validFrom: null, validTo: null));
            }
            var listtaxonomies = new List<LinkTaxonomyDto>();
            foreach (var taxonomy in taxonomies)
            {
                string linktaxonomyId = $"{serviceId}{location.Id.ToString().ToString()}{taxonomy.Id.ToString().ToString()}";
                if (!_linkTaxIds.Contains(linktaxonomyId))
                {
                    _linkTaxIds.Add(linktaxonomyId);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Duplicate");
                }

                string taxonomyId = $"{_adminAreaCode.Replace("E", "")}{taxonomy.Id.ToString().ToString()}";
                TaxonomyDto taxonomyItem = new TaxonomyDto(taxonomyId, taxonomy.Name.Trim(), taxonomyType: TaxonomyType.ServiceCategory, parent: null);
                if (!_dictTaxonomies.ContainsKey(taxonomyId))
                {
                    _organisationClientService.CreateTaxonomy(taxonomyItem);
                    _extraTaxonomies.Add(taxonomyItem);
                    _dictTaxonomies[taxonomyItem.Id]=taxonomyItem;
                }

                listtaxonomies.Add(new LinkTaxonomyDto(id: linktaxonomyId, serviceId, "Location", taxonomyItem));
            }

            var locationDto = new LocationDto(id: locationId, name: location.Name ?? locationId, description: location.Name ?? locationId, latitude: location.Geometry.Coordinates[1], longitude: location.Geometry.Coordinates[0], physicalAddresses: physicalAddresses, linkTaxonomies: listtaxonomies, linkContacts: new List<LinkContactDto>());

            var existingServiceAtLocation = list.FirstOrDefault(x => x.Id == locationId);
            if (existingServiceAtLocation != null)
            {
                //Replacement will be added below
                list.Remove(existingServiceAtLocation);
            }

            list.Add(new ServiceAtLocationDto(id: locationId, location: locationDto, regularSchedules: listRegularSchedules, holidaySchedules: new List<HolidayScheduleDto>(), linkContacts: new List<LinkContactDto>()));
        }

        return list;
    }

    private string GetValueFromDictionary(string key, Dictionary<string, JToken> data)
    {
        if (!data.ContainsKey(key))
        {
            return string.Empty;
        }

        if (data.TryGetValue(key, out JToken token) && token.ToString().Any())
        {
            return token.ToString();
        }

        return string.Empty;
    }
}