using ElmbridgeImporter.Services;
using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using IdGen;
using Newtonsoft.Json.Linq;
using ServiceDirectory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Xml.Linq;
using talentconsulting.open_referral_client.Interfaces;
using talentconsulting.open_referral_client.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using static System.Net.Mime.MediaTypeNames;

namespace ElmbridgeImporter;

public class ElmbridgeMapper
{
    private Dictionary<string, OrganisationWithServicesDto> _dictOrganisations;
    private Dictionary<string, TaxonomyDto> _dictTaxonomies;
    private readonly HashSet<string> _linkTaxIds;
    private readonly IElmbridgeClientService _elmbridgeClientService;
    private readonly IOrganisationClientService _organisationClientService;
    private const string _adminAreaCode = "E07000207";
 
    public string Name => "Elmbridge Mapper";

    public ElmbridgeMapper(IElmbridgeClientService elmbridgeClientService, IOrganisationClientService organisationClientService)
    {
        _elmbridgeClientService = elmbridgeClientService;
        _organisationClientService = organisationClientService;
        _linkTaxIds = new HashSet<string>();
    }

    public async Task AddOrUpdateServices()
    {
        const int startPage = 1;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        ElmbridgeSimpleService elmbridgeSimpleService = await _elmbridgeClientService.GetServicesByPage(startPage);
        foreach (var item in elmbridgeSimpleService.content)
        {
            ElmbridgeService elmbridgeService = await _elmbridgeClientService.GetServiceById(item.id);
        }
        //int totalPages = services.TotalPages;
        //int errors = await AddAndUpdateServices(services);
        //Console.WriteLine($"Completed Page {startPage} {totalPages} with {errors} errors");
        //for (int i = startPage + 1; i <= totalPages;  i++) 
        //{
        //    services = await _openReferralClient.GetPageServices(i);
        //    errors = await AddAndUpdateServices(services);

        //    Console.WriteLine($"Completed Page {i} of {totalPages} with {errors} errors");
        //}
    }

    private async Task<int> AddAndUpdateService(ElmbridgeService elmbridgeService)
    {
        List<string> errors = new List<string>();
        string serviceId = $"{_adminAreaCode.Replace("E", "")}{elmbridgeService.id}";
        OrganisationWithServicesDto serviceDirectoryOrganisation = default!;

        bool newOrganisation = false;
        string organisationId = $"{_adminAreaCode.Replace("E", "")}{elmbridgeService.organization.id}";
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
            name: elmbridgeService.organization.name,
            description: elmbridgeService.organization.name,
            logo: elmbridgeService.organization.logo,
            uri: elmbridgeService.organization.uri,
            url: elmbridgeService.organization.url,
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
            name: elmbridgeService.name,
            description: elmbridgeService.description,
            accreditations: elmbridgeService.accreditations,
            assuredDate: GetDateFromString(elmbridgeService.assured_date),
            attendingAccess: elmbridgeService.attending_access,
            attendingType: elmbridgeService.attending_type,
            deliverableType: elmbridgeService.deliverable_type,
            status: elmbridgeService.status,
            fees: elmbridgeService.fees,
            canFamilyChooseDeliveryLocation: false)
            .WithEligibility(GetEligibilityDtos(elmbridgeService.eligibilitys, existingService))
            .WithCostOption(GetCostOptionDtos(serviceId, elmbridgeService.cost_options, existingService))
            .WithServiceAreas(GetServiceAreas(elmbridgeService.service_areas, existingService))
            .WithFundings(GetFundings(elmbridgeService.fundings, existingService))
            .WithRegularSchedules(GetRegularSchedules(elmbridgeService.regular_schedules, existingService))
            .WithHolidaySchedules(GetHolidaySchedules(elmbridgeService.holiday_schedules, existingService))
            .WithLinkContact(GetLinkContactDtos(serviceId, elmbridgeService.contacts, existingService))
            .WithServiceAtLocations(GetServiceAtLocations(elmbridgeService.service_at_locations, existingService))
            .Build();

        foreach (string error in errors) 
        {
            Console.WriteLine(error);
        }

        return errors.Count;
    }

    private DateTime? GetDateFromString(string strDate)
    {
        if (string.IsNullOrEmpty(strDate))
            return null;

        if (DateTime.TryParse(strDate, out DateTime date))
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        return null;
    }

    private async Task<OrganisationWithServicesDto> InitialiseElmbridgeCouncil()
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        var elmbridgeCouncil = new OrganisationWithServicesDto(
        "ddafc1ea-089c-40ba-9b41-b1a8739fb628",
            new OrganisationTypeDto("1", "LA", "Local Authority"), "Elmbridge Council", "Elmbridge Council", null, new Uri("https://www.elmbridge.gov.uk/").ToString(), "https://www.elmbridge.gov.uk/", new List<ServiceDto>(), new List<LinkContactDto>());

        elmbridgeCouncil.AdminAreaCode = _adminAreaCode;

        await _organisationClientService.CreateOrganisation(elmbridgeCouncil);

        return elmbridgeCouncil;
#pragma warning restore S1075 // URIs should not be hardcoded
    }

    private async Task CreateOrganisationDictionary()
    {
        _dictOrganisations = new Dictionary<string, OrganisationWithServicesDto>();
        List<OrganisationDto> organisations = await _organisationClientService.GetListOrganisations();
        var elmbridge = organisations.FirstOrDefault(x => x.Name.Contains("Elmbridge"));
        if (elmbridge == null) 
        {
            var buckCouncil = await InitialiseElmbridgeCouncil();
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

    private List<EligibilityDto> GetEligibilityDtos(Eligibility[] eligibilities, ServiceDto existingService)
    {
        List<EligibilityDto> listEligibilityDto = new List<EligibilityDto>();

        if (existingService != null)
        {
            listEligibilityDto = existingService.Eligibilities.ToList();
        }

        foreach (Eligibility eligibility in eligibilities)
        {
            string eligibilityId = $"{_adminAreaCode.Replace("E", "")}{eligibility.id}";
            if (existingService != null)
            {
                EligibilityDto existing = existingService.Eligibilities.FirstOrDefault(x => x.Id == eligibility.id);
                if (existing != null)
                {
                    existingService.Eligibilities.Remove(existing);
                }
            }
            
            listEligibilityDto.Add(new EligibilityDto(id: eligibilityId, eligibilityDescription: eligibility.eligibility, maximumAge: eligibility.maximum_age, minimumAge: eligibility.minimum_age) );
        }

        return listEligibilityDto;

        
    }

    private List<CostOptionDto> GetCostOptionDtos(string serviceId, Cost_Options[] costOptions, ServiceDto existingService)
    {
        if (costOptions == null || !costOptions.Any())
        { 
            return new List<CostOptionDto>();
        }

        List<CostOptionDto> listCostOptionDto = new List<CostOptionDto>();
        if (existingService != null)
        {
            listCostOptionDto = existingService.CostOptions.ToList();
        }

        foreach (Cost_Options costOption in costOptions)
        {
            string costOptionId = $"{_adminAreaCode.Replace("E", "")}{costOption.id}";
            if (existingService != null)
            {
                CostOptionDto existing = existingService.CostOptions.FirstOrDefault(x => x.Id == costOptionId);
                if (existing != null)
                {
                    existingService.CostOptions.Remove(existing);
                }
            }
            
            listCostOptionDto.Add(new CostOptionDto(id: costOptionId, amountDescription: costOption.amount_description, amount: costOption.amount, linkId: serviceId, option: costOption.option, validFrom: GetDateFromString(costOption.valid_from), validTo: GetDateFromString(costOption.valid_to)));
        }

        return listCostOptionDto;
    }

    private List<ServiceAreaDto> GetServiceAreas(Service_Area[] serviceAreas, ServiceDto existingService)
    {
        if (serviceAreas == null || !serviceAreas.Any())
        {
            return new List<ServiceAreaDto>();
        }

        List<ServiceAreaDto> listServiceAreaDto = new List<ServiceAreaDto>();
        if (existingService != null)
        {
            listServiceAreaDto = existingService.ServiceAreas.ToList();
        }

        foreach (Service_Area serviceArea in serviceAreas)
        {
            string serviceAreaId = $"{_adminAreaCode.Replace("E", "")}{serviceArea.id}";
            if (existingService != null)
            {
                ServiceAreaDto existing = existingService.ServiceAreas.FirstOrDefault(x => x.Id == serviceAreaId);
                if (existing != null)
                {
                    existingService.ServiceAreas.Remove(existing);
                }
            }
            
            listServiceAreaDto.Add(new ServiceAreaDto(id: serviceAreaId, serviceAreaDescription: serviceArea.service_area, extent: serviceArea.extent, uri: null));
        }

        return listServiceAreaDto;
    }

    private List<FundingDto> GetFundings(Funding[] fundings, ServiceDto existingService)
    {
        if (fundings == null || !fundings.Any())
        {
            return new List<FundingDto>();
        }

        List<FundingDto> listFundingDto = new List<FundingDto>();

        if (existingService != null)
        {
            listFundingDto = existingService.Fundings.ToList();
        }

        foreach (Funding funding in fundings)
        {
            string fundingId = $"{_adminAreaCode.Replace("E", "")}{funding.id}";
            if (existingService != null)
            {
                FundingDto existing = existingService.Fundings.FirstOrDefault(x => x.Id == fundingId);
                if (existing != null)
                {
                    existingService.Fundings.Remove(existing);
                }
            }
            

            listFundingDto.Add(new FundingDto(id: fundingId, funding.source));
        }

        return listFundingDto;
    }

    private List<RegularScheduleDto> GetRegularSchedules(Regular_Schedule[] regularSchedules, ServiceDto existingService)
    {
        if (regularSchedules == null || !regularSchedules.Any())
        {
            return new List<RegularScheduleDto>();
        }

        List<RegularScheduleDto> listRegularScheduleDto = new List<RegularScheduleDto>();

        if (existingService != null)
        {
            listRegularScheduleDto = existingService.RegularSchedules.ToList();
        }

        foreach (Regular_Schedule regularSchedule in regularSchedules)
        {
            string regularScheduleId = $"{_adminAreaCode.Replace("E", "")}{regularSchedule.id}";
            if (existingService != null)
            {
                RegularScheduleDto existing = existingService.RegularSchedules.FirstOrDefault(x => x.Id == regularScheduleId);
                if (existing != null)
                {
                    existingService.RegularSchedules.Remove(existing);
                }
            }
            
            listRegularScheduleDto.Add(new RegularScheduleDto(id: regularScheduleId, description: regularSchedule.description, opensAt: GetDateFromString(regularSchedule.opens_at), closesAt: GetDateFromString(regularSchedule.closes_at), byDay: regularSchedule.byday, byMonthDay: regularSchedule.bymonthday, dtStart: regularSchedule.dtstart, freq: regularSchedule.freq, interval: regularSchedule.interval, validFrom: GetDateFromString(regularSchedule.valid_from), GetDateFromString(regularSchedule.valid_to)));
        }

        return listRegularScheduleDto;
    }

    private List<HolidayScheduleDto> GetHolidaySchedules(HolidaySchedule[] holidaySchedules, ServiceDto existingService)
    {
        if (holidaySchedules == null || !holidaySchedules.Any())
        {
            return new List<HolidayScheduleDto>();
        }

        List<HolidayScheduleDto> listHolidayScheduleDto = new List<HolidayScheduleDto>();
        if (existingService != null)
        {
            listHolidayScheduleDto = existingService.HolidaySchedules.ToList();
        }

        foreach (HolidaySchedule holidaySchedule in holidaySchedules)
        {
            string holidayScheduleId = $"{_adminAreaCode.Replace("E", "")}{holidaySchedule.id}";
            if (existingService != null)
            {
                HolidayScheduleDto existing = existingService.HolidaySchedules.FirstOrDefault(x => x.Id == holidayScheduleId);
                if (existing != null)
                {
                    existingService.HolidaySchedules.Remove(existing);
                }
            }
            
            bool.TryParse(holidaySchedule.closed, out bool closed);

            listHolidayScheduleDto.Add(new HolidayScheduleDto(id: holidayScheduleId, closed: closed, closesAt: GetDateFromString(holidaySchedule.closes_at), startDate: GetDateFromString(holidaySchedule.start_date), endDate: GetDateFromString(holidaySchedule.end_date), GetDateFromString(holidaySchedule.open_at)));
        }

        return listHolidayScheduleDto;
    }

    private List<RegularScheduleDto> GetServiceAtLocationRegularSchedules(Service_At_Location serviceAtLocation, ServiceDto existingService)
    {
        List<RegularScheduleDto> listRegularScheduleDto = new List<RegularScheduleDto>();
        foreach (Regular_Schedule regularSchedule in serviceAtLocation.regular_schedule)
        {
            string regularScheduleId = $"{_adminAreaCode.Replace("E", "")}{regularSchedule.id}";
            foreach (var item in existingService.ServiceAtLocations.Select(x => x.RegularSchedules))
            {
                RegularScheduleDto existing = item.FirstOrDefault(x => x.Id == regularScheduleId);
                if (existing != null)
                {
                    item.Remove(existing);
                }
            }

            listRegularScheduleDto.Add(new RegularScheduleDto(id: regularScheduleId, description: regularSchedule.description, opensAt: GetDateFromString(regularSchedule.opens_at), closesAt: GetDateFromString(regularSchedule.closes_at), byDay: regularSchedule.byday, byMonthDay: regularSchedule.bymonthday, dtStart: regularSchedule.dtstart, freq: regularSchedule.freq, interval: regularSchedule.interval, validFrom: GetDateFromString(regularSchedule.valid_from), GetDateFromString(regularSchedule.valid_to)));
        }

        return listRegularScheduleDto;
    }

    private List<HolidayScheduleDto> GetServiceAtLocationHolidaySchedules(Service_At_Location serviceAtLocation, ServiceDto existingService)
    {
        List<HolidayScheduleDto> listHolidayScheduleDto = new List<HolidayScheduleDto>();
        foreach (HolidaySchedule holidaySchedule in serviceAtLocation.holidayScheduleCollection)
        {
            string holidayScheduleId = $"{_adminAreaCode.Replace("E", "")}{holidaySchedule.id}";
            foreach (var item in existingService.ServiceAtLocations.Select(x => x.HolidaySchedules))
            {
                HolidayScheduleDto existing = item.FirstOrDefault(x => x.Id == holidayScheduleId);
                if (existing != null)
                {
                    item.Remove(existing);
                }
            }

            bool.TryParse(holidaySchedule.closed, out bool closed);

            listHolidayScheduleDto.Add(new HolidayScheduleDto(id: holidayScheduleId, closed: closed, closesAt: GetDateFromString(holidaySchedule.closes_at), startDate: GetDateFromString(holidaySchedule.start_date), endDate: GetDateFromString(holidaySchedule.end_date), GetDateFromString(holidaySchedule.open_at)));
        }

        return listHolidayScheduleDto;
    }

    private List<ServiceAtLocationDto> GetServiceAtLocations(Service_At_Location[] serviceAtLocations, ServiceDto existingService)
    {
        if (serviceAtLocations == null || !serviceAtLocations.Any())
        {
            return new List<ServiceAtLocationDto>();
        }

        List<ServiceAtLocationDto> listServiceAtLocationDto = new List<ServiceAtLocationDto>();
        if (existingService != null)
        {
            listServiceAtLocationDto = existingService.ServiceAtLocations.ToList();
        }

        foreach (Service_At_Location serviceAtLocation in serviceAtLocations) 
        {
            string Id = $"{_adminAreaCode.Replace("E", "")}{serviceAtLocation.location.id}";
            if (existingService != null)
            {
                ServiceAtLocationDto serviceAtLocationDto = existingService.ServiceAtLocations.FirstOrDefault(x => x.Id == Id);
                if (serviceAtLocationDto != null)
                {
                    existingService.ServiceAtLocations.Remove(serviceAtLocationDto);
                }
            }
            List<RegularScheduleDto>  regularSchedules = GetServiceAtLocationRegularSchedules(serviceAtLocation, existingService);
            List<HolidayScheduleDto> holidaySchedules = GetServiceAtLocationHolidaySchedules(serviceAtLocation, existingService);

            List<PhysicalAddressDto> physicalAddressDtos = new List<PhysicalAddressDto>();
            foreach(var physicalAddress in serviceAtLocation.location.physical_addresses)
            {
                string physicalAddressId = $"{_adminAreaCode.Replace("E", "")}{physicalAddress.address_1}{physicalAddress.postal_code}";
                physicalAddressId = physicalAddressId.Replace(" ", "-");
                physicalAddressDtos.Add(new PhysicalAddressDto(id: physicalAddressId, address1: physicalAddress.address_1, city: physicalAddress.city, postCode: physicalAddress.postal_code, country: physicalAddress.country, stateProvince: physicalAddress.state_province));

            }
            LocationDto location = new(id: Id, name: serviceAtLocation.location.name, description: serviceAtLocation.location.name, latitude: serviceAtLocation.location.latitude, longitude: serviceAtLocation.location.longitude,physicalAddresses: physicalAddressDtos, linkTaxonomies: new List<LinkTaxonomyDto>(), linkContacts: new List<LinkContactDto>() );

            listServiceAtLocationDto.Add(new ServiceAtLocationDto(id: Id,
                location: location,
                regularSchedules: regularSchedules,
                holidaySchedules: holidaySchedules,
                linkContacts: new List<LinkContactDto>()));
        }

        return listServiceAtLocationDto;
    }

    private List<LinkContactDto> GetLinkContactDtos(string serviceId, ElmbridgeImporter.Services.Contact[] contacts, ServiceDto existingService)
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
            string contactId = $"{_adminAreaCode.Replace("E", "")}{contact.id}";
            if (existingService != null) 
            {
                LinkContactDto existing = existingService.LinkContacts.FirstOrDefault(x => x.Contact.Id == contactId);
                if (existing != null)
                {
                    list.Remove(existing);
                }
            }

            string phone = default!;
            if (contact != null && contact.phones != null && contact.phones.Any())
            {
                phone = contact.phones.FirstOrDefault().number ?? string.Empty;
            }

            list.Add(new LinkContactDto(id: contactId, linkId: serviceId, linkType: null, 
                contact: new ContactDto(id: contactId, title: contact.title,name: contact.name, telephone: phone, phone,url: null, email: null)));


        }

        return list;
    }


    //private List<LinkContactDto> GetLinkContactDtos(string serviceId, IList<Contact> contacts, ServiceDto existingService)
    //{
    //    if (!contacts.Any())
    //    {
    //        return new List<LinkContactDto>();
    //    }

    //    var list = new List<LinkContactDto>();
    //    if (existingService != null) 
    //    {
    //        list = existingService.LinkContacts.ToList();
    //    }
    //    foreach (var contact in contacts)
    //    {
    //        if (string.IsNullOrEmpty(contact.Phone))
    //        {
    //            continue;
    //        }
    //        string contactId = $"{_adminAreaCode.Replace("E", "")}{contact.Id.ToString().ToString()}";
    //        if (existingService != null)
    //        {
    //            var linkContact = existingService.LinkContacts.FirstOrDefault(x => x.Id == contactId);
    //            if (linkContact != null)
    //            {
    //                //Will be replace by the one below
    //                existingService.LinkContacts.Remove(linkContact);
    //            }
    //        }

    //        list.Add(new LinkContactDto(id: contactId, linkId: serviceId, linkType: "ServiceContact", new ContactDto(id: contactId, title: contact.Title, name: contact.Name ?? "Contact", telephone: contact.Phone, textPhone: contact.Phone, url: null, email: contact.Email)));
    //    }

    //    return list;
    //}
}