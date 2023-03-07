using System;
using Newtonsoft.Json;

namespace talentconsulting.open_referral_client.Models
{
	public class RegularSchedules : BaseModel
	{
		public int Id { get; set; }

        public string Weekday { get; set; }

        [JsonProperty(PropertyName = "opens_at")]
        public string OpensAt { get; set; }

        [JsonProperty(PropertyName = "closes_at")]
        public string ClosesAt { get; set; }
    }
}