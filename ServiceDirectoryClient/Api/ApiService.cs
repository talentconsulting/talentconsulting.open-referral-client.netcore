namespace ServiceDirectoryClient.Api;

public interface IApiService
{

}


public class ApiService : IApiService
{
    protected readonly HttpClient _client;

    public ApiService(HttpClient client)
    {
        _client = client;
    }
}
