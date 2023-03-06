using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using talentconsulting.open_referral_client.Models;

namespace talentconsulting.open_referral_client.Interfaces
{
	public interface IManageOrganisations
	{
        Task<List<Organisation>> GetOrganisations<T>(T args);
    }
}

