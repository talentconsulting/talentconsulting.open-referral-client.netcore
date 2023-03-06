using System;
using System.Threading.Tasks;
using talentconsulting.open_referral_client.Interfaces;
using RestSharp;
using talentconsulting.open_referral_client.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace talentconsulting.open_referral_client
{
	public class OpenReferralClient : IOpenReferralClient
	{
        private RestClient _client;
        private string _basePath;

        public OpenReferralClient(Uri baseUri, string basePath) : this(basePath)
        {
            _client = new RestClient(baseUri);
        }

        public OpenReferralClient(Uri baseUri) : this("")
        {
            _client = new RestClient(baseUri);
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

        /// <summary>
        /// Supports 'taxonomy' and 'scheme'
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<List<Organisation>> GetOrganisations<T>(T args)
        {
            Dictionary<string, string> queryParamRequests;

            var request = new RestRequest($"{_basePath}/organizations");

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, args);
                stream.Seek(0, SeekOrigin.Begin);
                queryParamRequests = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream);
            }

            foreach (KeyValuePair<string, string> entry in queryParamRequests)
            {
                request.AddQueryParameter(entry.Key, entry.Value);
            }

            var result = await _client.GetAsync<List<Organisation>>(request);

            return result;
        }

        public async Task<ServiceResponse> GetServices<T>(T args)
        {
            Dictionary<string, string> queryParamRequests;

            var request = new RestRequest($"{_basePath}/services");

            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, args);
                stream.Seek(0, SeekOrigin.Begin);
                queryParamRequests = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream);
            }

            foreach (KeyValuePair<string, string> entry in queryParamRequests)
            {
                request.AddQueryParameter(entry.Key, entry.Value);
            }

            var result = await _client.GetAsync<ServiceResponse>(request);

            return result;
        }
    }
}

