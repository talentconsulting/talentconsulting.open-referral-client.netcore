using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using talentconsulting.open_referral_client.Interfaces;
using talentconsulting.open_referral_client.Models;

namespace talentconsulting.open_referral_client
{
    public class OpenReferralClient : IOpenReferralClient
	{
        private readonly RestClient _client;
        private readonly string _basePath;

        public OpenReferralClient(Uri baseUri, string basePath) : this(basePath)
        {
            _client = new RestClient(baseUri);
            _client.UseNewtonsoftJson();
        }

        public OpenReferralClient(Uri baseUri) : this("")
        {
            _client = new RestClient(baseUri);
            _client.UseNewtonsoftJson();
        }

        public OpenReferralClient(RestClient client, string basePath) : this(basePath)
        {
            _client = client;
        }

        public OpenReferralClient(RestClient client) : this("")
        {
            _client = client;
        }

        private OpenReferralClient(string basePath)
        {
            _basePath = basePath ?? "";
        }

        public void SetSerializerOptions(JsonSerializerSettings settings)
        {
            _client.UseNewtonsoftJson(settings);
        }

        public async Task<ServiceResponse> GetServices<T>(T args)
        {
            var request = new RestRequest($"{_basePath}/services");

            var queryParamRequests = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(args));

            foreach (KeyValuePair<string, string> entry in queryParamRequests)
            {
                request.AddQueryParameter(entry.Key, entry.Value);
            }

            var result = await _client.GetAsync<ServiceResponse>(request);

            return result;
        }
    }
}

