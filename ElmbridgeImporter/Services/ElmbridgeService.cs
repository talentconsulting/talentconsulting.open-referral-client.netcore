namespace ElmbridgeImporter.Services
{
    public class ElmbridgeService
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string email { get; set; }
        public string status { get; set; }
        public string fees { get; set; }
        public string accreditations { get; set; }
        public string deliverable_type { get; set; }
        public string attending_type { get; set; }
        public string attending_access { get; set; }
        public string pc_attendingAccess_additionalInfo { get; set; }
        public string assured_date { get; set; }
        public Service_Area[] service_areas { get; set; }
        public Funding[] fundings { get; set; }
        public Regular_Schedule[] regular_schedules { get; set; }
        public Eligibility[] eligibilitys { get; set; }
        public Service_At_Location[] service_at_locations { get; set; }
        public Cost_Options[] cost_options { get; set; }
        public object[] reviews { get; set; }
        public Organization organization { get; set; }
        public Contact[] contacts { get; set; }
        public HolidaySchedule[] holiday_schedules { get; set; }
        public Service_Taxonomys[] service_taxonomys { get; set; }
        public Language[] languages { get; set; }
        public Pc_Metadata pc_metadata { get; set; }
        public Pc_Targetaudience[] pc_targetAudience { get; set; }
    }

    public class Cost_Options
    {
        public string id { get; set; }
        public string service_id { get; set; }
        public string valid_from { get; set; }
        public string valid_to { get; set; }
        public string option { get; set; }
        public decimal amount { get; set; }
        public string amount_description { get; set; }
    }

    public class Eligibility
    {
        public string id { get; set; }
        public string service_id { get; set; }
        public string eligibility { get; set; }
        public int minimum_age { get; set; }
        public int maximum_age { get; set; }
    }

    public class Funding
    {
        public string id { get; set; }
        public string service_id { get; set; }
        public string source { get; set; }
    }

    public class Language
    {
        public string id { get; set; }
        public string service_id { get; set; }
        public string language { get; set; }
    }

    public class Service_Area
    {
        public string service_area { get; set; }
        public string extent { get; set; }
        public string id { get; set; }
    }

    public class Service_At_Location
    {
        public Regular_Schedule[] regular_schedule { get; set; }
        public HolidaySchedule[] holidayScheduleCollection { get; set; }
        public Location location { get; set; }
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

    public class Location
    {
        public Accessibility_For_Disabilities[] accessibility_for_disabilities { get; set; }
        public Physical_Addresses[] physical_addresses { get; set; }
        public string id { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string name { get; set; }
    }

    public class Accessibility_For_Disabilities
    {
        public string accessibility { get; set; }
    }

    public class Physical_Addresses
    {
        public string address_1 { get; set; }
        public string postal_code { get; set; }
        public string state_province { get; set; }
        public string city { get; set; }
        public string country { get; set; }
    }

    public class Regular_Schedule
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

    public class Contact
    {
        public string id { get; set; }
        public string name { get; set; }
        public Phone[] phones { get; set; }
        public string title { get; set; }
    }

    public class Phone
    {
        public string number { get; set; }
    }

    public class Service_Taxonomys
    {
        public string id { get; set; }
        public Taxonomy taxonomy { get; set; }
    }

    public class Taxonomy
    {
        public string id { get; set; }
        public string name { get; set; }
        public string vocabulary { get; set; }
    }

}
