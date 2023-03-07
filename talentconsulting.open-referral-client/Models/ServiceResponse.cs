using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace talentconsulting.open_referral_client.Models
{
    public class ServiceResponse: Pagination
    {
        [JsonProperty(PropertyName = "content")]
        public IList<Service> Services { get; set; }
    }
}

