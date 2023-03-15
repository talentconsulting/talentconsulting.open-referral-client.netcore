namespace ElmbridgeImporter.Services
{
    public class ElmbridgeSimpleService
    {
        public int totalElements { get; set; }
        public int totalPages { get; set; }
        public int number { get; set; }
        public int size { get; set; }
        public bool first { get; set; }
        public bool last { get; set; }
        public bool empty { get; set; }
        public Content[] content { get; set; }
    }

    public class Content
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
        public Organization organization { get; set; }
        public Pc_Metadata pc_metadata { get; set; }
        public Pc_Targetaudience[] pc_targetAudience { get; set; }
    }

    public class Organization
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string logo { get; set; }
        public string uri { get; set; }
    }

    public class Pc_Metadata
    {
        public string date_created { get; set; }
        public string date_modified { get; set; }
        public string date_assured { get; set; }
        public string assured_by { get; set; }
    }

    public class Pc_Targetaudience
    {
        public string id { get; set; }
        public string audienceType { get; set; }
    }

}
