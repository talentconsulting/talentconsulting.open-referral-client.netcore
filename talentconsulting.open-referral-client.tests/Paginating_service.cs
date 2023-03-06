using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using RestSharp;
using static System.Net.WebRequestMethods;
using static talentconsulting.open_referral_client.tests.Serialises_extra_properties;
using File = System.IO.File;

namespace talentconsulting.open_referral_client.tests
{
    public class Paginating_service
    {
        private OpenReferralClient _client;

        public Paginating_service()
        {
            var restClient = new RestClient(new HttpClient(new MockHttpMessageHandler())
            {
                BaseAddress = new Uri("https://mocked-api-for-open-referral.openreferral.org")
            });

            _client = new OpenReferralClient(restClient);
        }

        [Test]
        public void Fetches_first_page_of_service_defaults()
        {
            var serviceResponse = _client.GetServices(new {}).Result;

            Assert.That(serviceResponse.First, Is.True);
            Assert.That(serviceResponse.Last, Is.False);

            Assert.That(serviceResponse.TotalElements, Is.EqualTo(5857));
            Assert.That(serviceResponse.TotalPages, Is.EqualTo(118));

            Assert.That(serviceResponse.Size, Is.EqualTo(50));
        }
    }
}