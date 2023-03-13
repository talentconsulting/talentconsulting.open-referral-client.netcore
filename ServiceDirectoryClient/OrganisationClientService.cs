using FamilyHubs.ServiceDirectory.Shared.Dto;
using IdGen;
using RestSharp;

namespace ServiceDirectory;

public interface IOrganisationClientService
{
    Task<List<OrganisationDto>> GetListOrganisations();
    Task<OrganisationWithServicesDto> GetOrganisationById(string id);
    Task<string> CreateOrganisation(OrganisationWithServicesDto organisation);
    Task<string> UpdateOrganisation(OrganisationWithServicesDto organisation);
}

public class OrganisationClientService : IOrganisationClientService
{
    private readonly RestClient _client;

    public OrganisationClientService(string baseUri)
    {
        _client = new RestClient(baseUri);
    }

    public async Task<List<OrganisationDto>> GetListOrganisations()
    {
        var request = new RestRequest("api/organizations");

        return await _client.GetAsync<List<OrganisationDto>>(request, CancellationToken.None) ?? new List<OrganisationDto>();
    }

    public async Task<OrganisationWithServicesDto> GetOrganisationById(string id)
    {
        var request = new RestRequest($"api/organizations/{id}");

        return await _client.GetAsync<OrganisationWithServicesDto>(request, CancellationToken.None) ?? new OrganisationWithServicesDto(
            Guid.NewGuid().ToString()
            , new(string.Empty, string.Empty, string.Empty)
            , ""
            , null
            , null
            , null
            , null
            , null
            );
    }

    public async Task<string> CreateOrganisation(OrganisationWithServicesDto organisation)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(organisation);
        var request = new RestRequest($"api/organizations").AddJsonBody(json);
        request.RequestFormat = DataFormat.Json;

        await _client.PostAsync(request);

        return organisation.Id;
    }

    public async Task<string> UpdateOrganisation(OrganisationWithServicesDto organisation)
    {
        var request = new RestRequest($"api/organizations/{organisation.Id}").AddJsonBody(Newtonsoft.Json.JsonConvert.SerializeObject(organisation));
        request.RequestFormat = DataFormat.Json;

        await _client.PutAsync(request);

        return organisation.Id;
    }
}