using System;
using System.Collections.Generic;
using talentconsulting.open_referral_client.Interfaces;

namespace talentconsulting.open_referral_client.Models
{
    public class ServiceResponse: Pagination
    {
        public List<Service> Content { get; set; }
    }
}

