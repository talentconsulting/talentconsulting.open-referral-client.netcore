using System.IO;
using System.Reflection;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using talentconsulting.open_referral_client.Models;

namespace talentconsulting.open_referral_client.tests
{
    public partial class Serialises_extra_properties
    {
        private OpenReferralClient _client;

        public Serialises_extra_properties()
        {
            var restClient = new RestClient(new HttpClient(new MockHttpMessageHandler())
            {
                BaseAddress = new Uri("https://mocked-api-for-open-referral.openreferral.org//sds")
            });

            restClient.UseNewtonsoftJson();

            _client = new OpenReferralClient(restClient, "");
        }

        [Test]
        public void Fetches_all_properties_for_a_service()
        {
            var serviceResponse = _client.GetServices(
                new { }
            ).Result;

            var count = 0;

            serviceResponse.Services.First().Data.ToList().ForEach(kvp =>
            {
                Assert.IsNotNull(kvp.Value);
                Assert.IsNotNull(serviceResponse.Services[count++].Description);
            });
        }

        [Test]
        public void Serialises_object_tree()
        {
            var serviceResponse = _client.GetServices(
                new { }
            ).Result;

            Assert.That(serviceResponse.Services.Last().Locations.First().Id, Is.EqualTo(6225));
        }
    }
}