using System;
using System.Threading.Tasks;
using talentconsulting.open_referral_client.Interfaces;
using RestSharp;
using talentconsulting.open_referral_client.Models;
using System.Collections.Generic;

namespace talentconsulting.open_referral_client
{
	public class OpenReferralClient : IOpenReferralClient
	{
        private RestClient _client;

        public OpenReferralClient(Uri baseUri)
        {
            _client = new RestClient(baseUri);
        }

        public OpenReferralClient(RestClient client)
        {
            _client = client;
        }

        public async Task<List<Organisation>> GetOrganisations()
        {
            var result = await _client.GetAsync<List<Organisation>>(new RestRequest("o/ServiceDirectoryService/v2/organizations"));

            return result;
        }
    }
}

