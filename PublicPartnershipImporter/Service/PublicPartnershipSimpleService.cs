using System;

namespace PublicPartnershipImporter.Service
{
    public class PublicPartnershipSimpleService
    {
        public int totalelements { get; set; }
        public int totalpages { get; set; }
        public int number { get; set; }
        public int size { get; set; }
        public bool first { get; set; }
        public bool last { get; set; }
        public Content[] content { get; set; }
    }

    public class Content
    {
        public string id { get; set; }
        public object accreditations { get; set; }
        public string assured_date { get; set; }
        public object attending_access { get; set; }
        public object attending_type { get; set; }
        public Contact[] contacts { get; set; }
        public CostOptions[] cost_options { get; set; }
        public string deliverable_type { get; set; }
        public string description { get; set; }
        public Eligibility[] eligibilitys { get; set; }
        public string email { get; set; }
        public string fees { get; set; }
        public Funding[] fundings { get; set; }
        public HolidaySchedule[] holiday_schedules { get; set; }
        public Language[] languages { get; set; }
        public string name { get; set; }
        public RegularSchedule[] regular_schedules { get; set; }
        public Review[] reviews { get; set; }
        public ServiceAreas[] service_areas { get; set; }
        public ServiceAtLocations[] service_at_locations { get; set; }
        public ServiceTaxonomys[] service_taxonomys { get; set; }
        public string status { get; set; }
        public object url { get; set; }
        public Organization organization { get; set; }
    }

    public class Organization
    {
        public string description { get; set; }
        public string id { get; set; }
        public string logo { get; set; }
        public string name { get; set; }
        public object reviews { get; set; }
        public object services { get; set; }
        public string uri { get; set; }
        public string url { get; set; }
    }

    public class Contact
    {
        public string id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public Phone[] phones { get; set; }
    }

    public class Phone
    {
        public string id { get; set; }
        public string number { get; set; }
    }

    public class CostOptions
    {
        public decimal amount { get; set; }
        public string amount_description { get; set; }
        public string id { get; set; }
        public string linkid { get; set; }
        public string option { get; set; }
        public string valid_from { get; set; }
        public string valid_to { get; set; }
    }

    public class Eligibility
    {
        public string eligibility { get; set; }
        public string id { get; set; }
        public string linkid { get; set; }
        public string maximum_age { get; set; }
        public string minimum_age { get; set; }
    }

    public class Funding
    {
        public string id { get; set; }
        public string source { get; set; }
    }

    public class Review
    {
        public DateTime date { get; set; }
        public string description { get; set; }
        public string id { get; set; }
        public string score { get; set; }
        public object service { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string widget { get; set; }
    }

    public class ServiceAreas
    {
        public string extent { get; set; }
        public string id { get; set; }
        public object linkid { get; set; }
        public string service_area { get; set; }
        public object uri { get; set; }
    }

    public class ServiceAtLocations
    {
        public HolidaySchedule[] holiday_schedules { get; set; }
        public string id { get; set; }
        public Location location { get; set; }
        public RegularSchedule[] regular_schedule { get; set; }
    }

    public class HolidaySchedule
    {
        public string id { get; set; }
        public string service_id { get; set; }
        public string service_at_location_id { get; set; }
        public string closed { get; set; }
        public string open_at { get; set; }
        public string closes_at { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }

    }

    public class RegularSchedule
    {
        public string closes_at { get; set; }
        public string opens_at { get; set; }
        public string valid_from { get; set; }
        public string valid_to { get; set; }
        public string byday { get; set; }
        public string bymonthday { get; set; }
        public string description { get; set; }
        public string dtstart { get; set; }
        public string freq { get; set; }
        public string id { get; set; }
        public string interval { get; set; }
    }

    public class Location
    {
        public object accessibility_for_disabilities { get; set; }
        public string description { get; set; }
        public string id { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string name { get; set; }
        public PhysicalAddresses[] physical_addresses { get; set; }
    }

    public class PhysicalAddresses
    {
        public string address_1 { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string id { get; set; }
        public string postal_code { get; set; }
        public string state_province { get; set; }
    }

    public class ServiceTaxonomys
    {
        public string id { get; set; }
        public object linkid { get; set; }
        public Taxonomy taxonomy { get; set; }
    }

    public class Taxonomy
    {
        public string id { get; set; }
        public string name { get; set; }
        public string vocabulary { get; set; }
        public object linktaxonomycollection { get; set; }
        public object servicetaxonomycollection { get; set; }
    }

    public class Language
    {
        public string id { get; set; }
        public string service_id { get; set; }
        public string language { get; set; }
    }
}
