namespace SouthamptonImporter.Service
{
    public class SouthamptonSimpleService
    {
        public int totalElements { get; set; }
        public int totalPages { get; set; }
        public int number { get; set; }
        public int size { get; set; }
        public bool last { get; set; }
        public bool first { get; set; }
        public bool empty { get; set; }
        public Content[] content { get; set; }
    }

    public class Content
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string referral_notes { get; set; }
        public bool? needs_referral { get; set; }
        public string ofsted_urn { get; set; }
        public string ofsted_url { get; set; }
        public OfstedLatestInspection ofsted_latest_inspection { get; set; }
        public string cqc_url { get; set; }
        public string schedule_description { get; set; }
        public bool? eligible_for2_yo_funding { get; set; }
        public bool? eligible_for34_yo_funding { get; set; }
        public object eligible_for_haf_funding { get; set; }
        public bool? eligible_for30_h_entitlement { get; set; }
        public DateTime assured_date { get; set; }
        public ServiceAtLocations[] service_at_locations { get; set; }
        public Organization organization { get; set; }
        public CostOptions[] cost_options { get; set; }
        public RegularSchedule[] regular_schedules { get; set; }
        public Language[] languages { get; set; }
        public ServiceAreas[] service_areas { get; set; }
        public SchoolPickups[] school_pickups { get; set; }
        public Link[] links { get; set; }
        public Special_Needs[] special_needs { get; set; }
        public Contact[] contacts { get; set; }
        public Eligibility[] eligibilitys { get; set; }
        public ServiceTaxonomys[] service_taxonomys { get; set; }
        public string status { get; set; }
        public object distance_away { get; set; }
        public object distance_away_is_service_area { get; set; }
        public object alert { get; set; }
        public LocalOffer local_offer { get; set; }
        public string cqc_current_rating { get; set; }
        public object cqc_last_inspection { get; set; }
        public string extra_section_heading { get; set; }
        public string extra_section_content { get; set; }
        public bool hide_address { get; set; }
    }

    public class OfstedLatestInspection
    {
        public string type { get; set; }
        public DateTime date { get; set; }
        public string overall_judgement { get; set; }
    }

    public class Organization
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class LocalOffer
    {
        public string description { get; set; }
        public string link { get; set; }
    }

    public class ServiceAtLocations
    {
        public object id { get; set; }
        public Location location { get; set; }
    }

    public class Location
    {
        public string id { get; set; }
        public string name { get; set; }
        public PhysicalAddresses[] physical_addresses { get; set; }
        public float? latitude { get; set; }
        public float? longitude { get; set; }
        public object[] accessibilities { get; set; }
    }

    public class PhysicalAddresses
    {
        public string address1 { get; set; }
        public string city { get; set; }
        public string state_province { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public string id { get; set; }
    }

    public class CostOptions
    {
        public string option { get; set; }
        public decimal amount { get; set; }
        public string amount_description { get; set; }
        public string id { get; set; }
    }

    public class RegularSchedule
    {
        public string weekday { get; set; }
        public string opens_at { get; set; }
        public string closes_at { get; set; }
        public string id { get; set; }
    }

    public class Language
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class ServiceAreas
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class SchoolPickups
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Link
    {
        public string label { get; set; }
        public string url { get; set; }
    }

    public class Special_Needs
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Contact
    {
        public string id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string email { get; set; }
        public Phone[] phones { get; set; }
    }

    public class Phone
    {
        public string id { get; set; }
        public string number { get; set; }
    }

    public class Eligibility
    {
        public string id { get; set; }
        public int? minimum_age { get; set; }
        public int? maximum_age { get; set; }
        public string eligibility { get; set; }
    }

    public class ServiceTaxonomys
    {
        public string id { get; set; }
        public Taxonomy taxonomy { get; set; }
    }

    public class Taxonomy
    {
        public string id { get; set; }
        public string name { get; set; }
        public string vocabulary { get; set; }
        public object prerequisite_taxonomies { get; set; }
    }

}
