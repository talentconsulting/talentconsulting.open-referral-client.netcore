using System;
namespace talentconsulting.open_referral_client.Models
{
    public class Geometry : BaseModel
	{
        public string Type { get; set; }

        public float[] Coordinates { get; set; }
	}
}

