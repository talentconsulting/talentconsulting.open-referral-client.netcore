using Newtonsoft.Json;

namespace talentconsulting.open_referral_client.Models
{
    public class Taxonomy : BaseModel
	{
		public int Id { get; set; }

		public string Name { get; set; }

        public string Slug { get; set; }

        [JsonProperty(PropertyName = "parent_id")]
        public string ParentId { get; set; }
    }
}