using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using talentconsulting.open_referral_client.Models;

namespace talentconsulting.open_referral_client.Interfaces
{
	public interface IManageServices
	{
        Task<ServiceResponse> GetServices<T>(T args);
        Task<ServiceResponse> GetPageServices(int page);
    }
}

