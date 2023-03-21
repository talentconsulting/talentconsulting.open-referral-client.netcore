using RestSharp;

namespace SouthamptonImporter.Service;

public interface ISouthamptonClientService
{
    Task<SouthamptonSimpleService> GetServicesByPage(int pageNumber);
}

public class SouthamptonClientService : ISouthamptonClientService
{
    private readonly RestClient _client;

    public SouthamptonClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<SouthamptonSimpleService> GetServicesByPage(int pageNumber)
    {
        var request = new RestRequest($"services/?&page={pageNumber}");

        return await _client.GetAsync<SouthamptonSimpleService>(request, CancellationToken.None) ?? new SouthamptonSimpleService();
    }
}

