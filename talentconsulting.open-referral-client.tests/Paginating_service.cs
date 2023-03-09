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
    public class Paginating__of_services
    {
        private OpenReferralClient _client;

        public Paginating__of_services()
        {
            var restClient = new RestClient(new HttpClient(new MockHttpMessageHandler())
            {
                BaseAddress = new Uri("https://mocked-api-for-open-referral.openreferral.org"),
            });

            restClient.UseNewtonsoftJson();

            _client = new OpenReferralClient(restClient);
        }

        [Test]
        public void Fetches_first_page_of_service_defaults()
        {
            var serviceResponse = _client.GetServices(new {}).Result;

            Assert.That(serviceResponse.First, Is.True);
            Assert.That(serviceResponse.Last, Is.False);
            Assert.That(serviceResponse.Number, Is.EqualTo(1));

            Assert.That(serviceResponse.TotalElements, Is.EqualTo(5875));
            Assert.That(serviceResponse.TotalPages, Is.EqualTo(118));

            Assert.That(serviceResponse.Size, Is.EqualTo(50));
        }

        [Test]
        public void Fetches_second_page_of_service()
        {
            var serviceResponse = _client.GetServices(
                new
                {
                    page = "2"
                }).Result;

            Assert.That(serviceResponse.First, Is.False);
            Assert.That(serviceResponse.Last, Is.False);
            Assert.That(serviceResponse.Number, Is.EqualTo(2));
        }
    }
}