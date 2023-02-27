using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace talentconsulting.open_referral_client.Models
{
    public class BaseModel 
    {
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}

