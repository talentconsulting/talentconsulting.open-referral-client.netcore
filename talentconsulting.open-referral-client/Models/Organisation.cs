using System;

namespace talentconsulting.open_referral_client.Models
{
    public class Organisation: BaseModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}

