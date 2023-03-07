using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace talentconsulting.open_referral_client.Models
{
    public class BaseModel 
    {
        [JsonExtensionData]
        public Dictionary<string, JToken> Data { get; set; }
    }
}

