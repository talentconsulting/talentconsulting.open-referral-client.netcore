using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using static System.Net.WebRequestMethods;
using static talentconsulting.open_referral_client.tests.Serialises_extra_properties;
using File = System.IO.File;

namespace talentconsulting.open_referral_client.tests
{
    public class Service_management
    {
        private OpenReferralClient _client;

        public Service_management()
        {
            var restClient = new RestClient(new HttpClient(new MockHttpMessageHandler())
            {
                BaseAddress = new Uri("https://mocked-api-for-open-referral.openreferral.org"),
            });

            restClient.UseNewtonsoftJson();

            _client = new OpenReferralClient(restClient);
        }

        [Test]
        public void Fetches_a_nonexistant_service()
        {
            var serviceResponse = _client.GetService("unknown").Result;

            Assert.IsNull(serviceResponse);
        }

        [Test]
        public void Fetches_an_existing_service()
        {
            var serviceResponse = _client.GetService("1234").Result;

            Assert.IsNotNull(serviceResponse);
        }
    }
}