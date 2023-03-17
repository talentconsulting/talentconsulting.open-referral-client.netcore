using RestSharp;
using System.Threading;
using System.Threading.Tasks;

namespace ElmbridgeImporter.Services
{
    public interface IPlacecubeClientService
    {
        Task<PlacecubeSimpleService> GetServicesByPage(int pageNumber);
        Task<PlacecubeService> GetServiceById(string id);
    }

    public class PlacecubeClientService : IPlacecubeClientService
    {
        private readonly RestClient _client;

        public PlacecubeClientService(string baseUri)
        {
            _client = new RestClient(baseUri);
        }

        public async Task<PlacecubeSimpleService> GetServicesByPage(int pageNumber)
        {
            var request = new RestRequest($"services/?&page={pageNumber}");

            return await _client.GetAsync<PlacecubeSimpleService>(request, CancellationToken.None) ?? new PlacecubeSimpleService();
        }

        public async Task<PlacecubeService> GetServiceById(string id)
        {
            var request = new RestRequest($"services/{id}");

            return await _client.GetAsync<PlacecubeService>(request, CancellationToken.None) ?? new PlacecubeService();
        }
    }
}
