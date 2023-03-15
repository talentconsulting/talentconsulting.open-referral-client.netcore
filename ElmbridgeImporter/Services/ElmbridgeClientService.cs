using RestSharp;
using System.Threading;
using System.Threading.Tasks;

namespace ElmbridgeImporter.Services
{
    public interface IElmbridgeClientService
    {
        Task<ElmbridgeSimpleService> GetServicesByPage(int pageNumber);
        Task<ElmbridgeService> GetServiceById(string id);
    }

    public class ElmbridgeClientService : IElmbridgeClientService
    {
        private readonly RestClient _client;

        public ElmbridgeClientService(string baseUri)
        {
            _client = new RestClient(baseUri);
        }

        public async Task<ElmbridgeSimpleService> GetServicesByPage(int pageNumber)
        {
            var request = new RestRequest($"services/?&page={pageNumber}");

            return await _client.GetAsync<ElmbridgeSimpleService>(request, CancellationToken.None) ?? new ElmbridgeSimpleService();
        }

        public async Task<ElmbridgeService> GetServiceById(string id)
        {
            var request = new RestRequest($"services/{id}");

            return await _client.GetAsync<ElmbridgeService>(request, CancellationToken.None) ?? new ElmbridgeService();
        }
    }
}
