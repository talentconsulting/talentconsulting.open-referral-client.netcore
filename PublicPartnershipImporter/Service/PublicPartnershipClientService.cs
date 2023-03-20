using RestSharp;
using System.Threading;
using System.Threading.Tasks;

namespace PublicPartnershipImporter.Service;

public interface IPublicPartnershipClientService
{
    Task<PublicPartnershipSimpleService> GetServicesByPage(int pageNumber);
}

public class PublicPartnershipClientService : IPublicPartnershipClientService
{
    private readonly RestClient _client;

    public PublicPartnershipClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<PublicPartnershipSimpleService> GetServicesByPage(int pageNumber)
    {
        var request = new RestRequest($"services/?&page={pageNumber}");

        return await _client.GetAsync<PublicPartnershipSimpleService>(request, CancellationToken.None) ?? new PublicPartnershipSimpleService();
    }
}
