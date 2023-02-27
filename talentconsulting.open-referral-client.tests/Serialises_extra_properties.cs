using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using RestSharp;
using static System.Net.WebRequestMethods;
using static talentconsulting.open_referral_client.tests.Serialises_extra_properties;

namespace talentconsulting.open_referral_client.tests;

public class Serialises_extra_properties
{
    private OpenReferralClient _client;

    public Serialises_extra_properties()
    {
        var restClient = new RestClient(new HttpClient(new MockHttpMessageHandler())
        {
            BaseAddress = new Uri("https://bristol.openplace.directory")
        });

        _client = new OpenReferralClient(restClient);
    }

    [Test]
    public void Fetches_all_properties_for_an_organisation()
    {
        var organisations = _client.GetOrganisations().Result;

        var count = 0;

        organisations.First().Data.ToList().ForEach(kvp =>
        {
            Assert.IsNotNull(kvp.Value);
            Assert.IsNotNull(organisations[count++].Description);
        });
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            switch(request.RequestUri.LocalPath)
            {
                case "/o/ServiceDirectoryService/v2/organizations":

                    var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        RequestMessage = request,
                        Content = new StringContent(System.IO.File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "canned-responses/organisations.json")).Result, Encoding.UTF8, "application/json")                 
                    };

                    return await Task.FromResult(responseMessage);
            }

            throw new NotImplementedException("Interface not mocked");
        }
    }
}
