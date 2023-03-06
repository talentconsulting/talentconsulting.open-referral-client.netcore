using System.IO;
using System.Reflection;
using RestSharp;

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

            _client = new OpenReferralClient(restClient, "");
        }

        [Test]
        public void Fetches_all_properties_for_an_organisation()
        {
            var organisations = _client.GetOrganisations(
                new { }
            ).Result;

            var count = 0;

            organisations.First().Data.ToList().ForEach(kvp =>
            {
                Assert.IsNotNull(kvp.Value);
                Assert.IsNotNull(organisations[count++].Description);
            });
        }
    }
}