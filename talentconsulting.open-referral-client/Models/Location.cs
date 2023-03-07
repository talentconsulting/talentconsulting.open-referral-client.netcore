using System.Collections.Generic;

namespace talentconsulting.open_referral_client.Models
{

    public class Location : BaseModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Label { get; set; }

        public Geometry Geometry { get; set; }

        public IList<Accessibility> Accessibilities { get; set; }
    }
}