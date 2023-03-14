using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace talentconsulting.open_referral_client.Models
{

    public class Service: ServiceBase
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Organisation Organisation { get; set; }

        [JsonProperty("organization")]
        private Organisation CodeModel2 { set { Organisation = value; } }

        [JsonProperty(PropertyName = "target_directories")]
        public IList<TargetDirectory> TargetDirectories { get; set; }

        public IList<Location> Locations { get; set; }

        public IList<Contact> Contacts { get; set; }

        public IList<Meta> Meta { get; set; }

        public IList<Taxonomy> Taxonomies { get; set; }

        [JsonProperty(PropertyName = "regular_schedules")]
        public IList<RegularSchedules> RegularSchedules { get; set; }

        [JsonProperty(PropertyName = "distance_away")]
        public string DistanceAway { get; set; }
    }
}

