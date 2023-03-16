using ElmbridgeImporter.Services;
using FamilyHubs.ServiceDirectory.Shared.Builders;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using Pipelines.Sockets.Unofficial.Arenas;
using ServiceDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElmbridgeImporter;

public class ElmbridgeMapper
{
    private Dictionary<string, OrganisationWithServicesDto> _dictOrganisations;
    private Dictionary<string, TaxonomyDto> _dictTaxonomies;
    private readonly Dictionary<string, ServiceTaxonomyDto> _dictServiceTaxonomies;
    private List<TaxonomyDto> _alltaxonomies;
    private readonly IElmbridgeClientService _elmbridgeClientService;
    private readonly IOrganisationClientService _organisationClientService;
    private const string _adminAreaCode = "E07000207";
 
    public string Name => "Elmbridge Mapper";

    public ElmbridgeMapper(IElmbridgeClientService elmbridgeClientService, IOrganisationClientService organisationClientService)
    {
        _elmbridgeClientService = elmbridgeClientService;
        _organisationClientService = organisationClientService;
        _dictServiceTaxonomies = new Dictionary<string, ServiceTaxonomyDto>();
    }

    public async Task AddOrUpdateServices()
    {
        const int startPage = 1;
        await CreateOrganisationDictionary();
        await CreateTaxonomyDictionary();
        ElmbridgeSimpleService elmbridgeSimpleService = await _elmbridgeClientService.GetServicesByPage(startPage);
        int totalPages = elmbridgeSimpleService.totalPages;
        int errorCount = 0;
        foreach (var item in elmbridgeSimpleService.content)
        {
            try
            {
                ElmbridgeService elmbridgeService = await _elmbridgeClientService.GetServiceById(item.id);
                errorCount += await AddAndUpdateService(elmbridgeService);
            }
            catch
            {
                Console.WriteLine($"Failed to get service for id: {item.id}");
            }
            
        }
        Console.WriteLine($"Completed Page {startPage} {totalPages} with {errorCount} errors"); 
        for (int i = startPage + 1; i <= totalPages;  i++) 
        {
            errorCount = 0;
            elmbridgeSimpleService = await _elmbridgeClientService.GetServicesByPage(i);
            foreach (var item in elmbridgeSimpleService.content)
            {
                try
                {
                    ElmbridgeService elmbridgeService = await _elmbridgeClientService.GetServiceById(item.id);
                    errorCount += await AddAndUpdateService(elmbridgeService);
                }
                catch
                {
                    Console.WriteLine($"Failed to get service for id: {item.id}");
                }
            }

            Console.WriteLine($"Completed Page {i} of {totalPages} with {errorCount} errors");
        }
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
            .WithLanguages(GetLanguages(elmbridgeService.languages, existingService))
            .WithServiceTaxonomies(GetServiceTaxonomies(elmbridgeService.service_taxonomys, existingService))
            .Build();

        if (serviceId == "07000207f26784ce-0099-4f16-b314-718d83ad76fd")
        {
            System.Diagnostics.Debug.WriteLine("Got Here");
        }

        if (newOrganisation)
        {
            //Create Organisation
            serviceDirectoryOrganisation.Services.Add(newService);

            try
            {
                await _organisationClientService.CreateOrganisation(serviceDirectoryOrganisation);
            }
            catch (Exception ex)
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
        var allTaxonomies = await _organisationClientService.GetTaxonomyList(1, 9999);
        _alltaxonomies = allTaxonomies.Items;
        foreach (var taxonomy in allTaxonomies.Items)
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
            
            EligibilityDto existing = listEligibilityDto.FirstOrDefault(x => x.Id == eligibility.id);
            if (existing != null)
            {
                listEligibilityDto.Remove(existing);
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
            if (string.IsNullOrEmpty(costOption.amount_description))
                continue;

            string costOptionId = $"{_adminAreaCode.Replace("E", "")}{costOption.id}";
            
            CostOptionDto existing = listCostOptionDto.FirstOrDefault(x => x.Id == costOptionId);
            if (existing != null)
            {
                listCostOptionDto.Remove(existing);
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
            
            ServiceAreaDto existing = listServiceAreaDto.FirstOrDefault(x => x.Id == serviceAreaId);
            if (existing != null)
            {
                listServiceAreaDto.Remove(existing);
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
            FundingDto existing = listFundingDto.FirstOrDefault(x => x.Id == fundingId);
            if (existing != null)
            {
                listFundingDto.Remove(existing);
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
            RegularScheduleDto existing = listRegularScheduleDto.FirstOrDefault(x => x.Id == regularScheduleId);
            if (existing != null)
            {
                listRegularScheduleDto.Remove(existing);
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
            HolidayScheduleDto existing = listHolidayScheduleDto.FirstOrDefault(x => x.Id == holidayScheduleId);
            if (existing != null)
            {
                listHolidayScheduleDto.Remove(existing);
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
            if (existingService != null) 
            {
                foreach (var item in existingService.ServiceAtLocations.Select(x => x.RegularSchedules))
                {
                    RegularScheduleDto existing = item.FirstOrDefault(x => x.Id == regularScheduleId);
                    if (existing != null)
                    {
                        item.Remove(existing);
                    }
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
            if (existingService != null) 
            {
                foreach (var item in existingService.ServiceAtLocations.Select(x => x.HolidaySchedules))
                {
                    HolidayScheduleDto existing = item.FirstOrDefault(x => x.Id == holidayScheduleId);
                    if (existing != null)
                    {
                        item.Remove(existing);
                    }
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
            ServiceAtLocationDto serviceAtLocationDto = listServiceAtLocationDto.FirstOrDefault(x => x.Id == Id);
            if (serviceAtLocationDto != null)
            {
                listServiceAtLocationDto.Remove(serviceAtLocationDto);
            }
            
            List<RegularScheduleDto>  regularSchedules = GetServiceAtLocationRegularSchedules(serviceAtLocation, existingService);
            List<HolidayScheduleDto> holidaySchedules = GetServiceAtLocationHolidaySchedules(serviceAtLocation, existingService);

            List<PhysicalAddressDto> physicalAddressDtos = new List<PhysicalAddressDto>();
            foreach(var physicalAddress in serviceAtLocation.location.physical_addresses)
            {
                string physicalAddressId = $"{_adminAreaCode.Replace("E", "")}{serviceAtLocation.location.id}{physicalAddress.address_1}{physicalAddress.postal_code}";
                physicalAddressId = physicalAddressId.Replace(",", "").Trim();
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
            LinkContactDto existing = list.FirstOrDefault(x => x.Contact.Id == contactId);
            if (existing != null)
            {
                list.Remove(existing);
            }
           
            string phone = default!;
            if (contact != null && contact.phones != null && contact.phones.Any())
            {
                phone = contact.phones.FirstOrDefault().number ?? string.Empty;
            }

            if (string.IsNullOrEmpty(phone))
            {
                continue;
            }

            list.Add(new LinkContactDto(id: contactId, linkId: serviceId, linkType: "ServiceContact", 
                contact: new ContactDto(id: contactId, title: contact.title,name: contact.name, telephone: phone, phone,url: null, email: null)));
        }

        return list;
    }


    private List<LanguageDto> GetLanguages(Language[] languages, ServiceDto existingService)
    {
        if (languages == null || !languages.Any())
        {
            return new List<LanguageDto>();
        }

        List<LanguageDto> listLanguageDto = new List<LanguageDto>();
        if (existingService != null)
        {
            listLanguageDto = existingService.Languages.ToList();
        }

        foreach (Language language in languages)
        {
            string serviceAreaId = $"{_adminAreaCode.Replace("E", "")}{language.id}";
            LanguageDto existing = listLanguageDto.FirstOrDefault(x => x.Id == serviceAreaId);
            if (existing != null)
            {
                listLanguageDto.Remove(existing);
            }
            
            listLanguageDto.Add(new LanguageDto(id: serviceAreaId, name: language.language));
        }

        return listLanguageDto;
    }

    private List<ServiceTaxonomyDto> GetServiceTaxonomies(Service_Taxonomys[] serviceTaxonomies, ServiceDto existingService)
    {
        if (serviceTaxonomies == null || !serviceTaxonomies.Any())
        {
            return new List<ServiceTaxonomyDto>();
        }

        List<ServiceTaxonomyDto> listServiceTaxonomyDto = new List<ServiceTaxonomyDto>();
        if (existingService != null)
        {
            listServiceTaxonomyDto = existingService.ServiceTaxonomies.ToList();
        }

        foreach (Service_Taxonomys serviceTaxonomy in serviceTaxonomies)
        {
            string serviceTaxonomyId = $"{_adminAreaCode.Replace("E", "")}{serviceTaxonomy.id}";

            if (serviceTaxonomyId == "07000207MentalIllness")
            {
                System.Diagnostics.Debug.WriteLine("Got Here");
            }

            ServiceTaxonomyDto existing = listServiceTaxonomyDto.FirstOrDefault(x => x.Id == serviceTaxonomyId);
            if (existing != null)
            {
                listServiceTaxonomyDto.Remove(existing);
            }

            string taxonomyId = $"{_adminAreaCode.Replace("E", "")}{serviceTaxonomy.taxonomy.id}";
            TaxonomyDto taxonomyDto = new TaxonomyDto(id: taxonomyId, name: serviceTaxonomy.taxonomy.name, taxonomyType: TaxonomyType.ServiceCategory, parent: null);
            if (!_dictTaxonomies.ContainsKey(taxonomyId))
            {
                var existingTaxonomy = _alltaxonomies.FirstOrDefault(x => x.Name.ToLower() == taxonomyDto.Name.ToLower());
                if (existingTaxonomy != null) 
                {
                    taxonomyDto.Id = existingTaxonomy.Id;
                }
                else
                {
                    _organisationClientService.CreateTaxonomy(taxonomyDto);
                    _dictTaxonomies[taxonomyId] = taxonomyDto;
                }  
            }

            listServiceTaxonomyDto.Add(new ServiceTaxonomyDto(id: serviceTaxonomyId, taxonomy: taxonomyDto));
        }

        return listServiceTaxonomyDto;
    }
}