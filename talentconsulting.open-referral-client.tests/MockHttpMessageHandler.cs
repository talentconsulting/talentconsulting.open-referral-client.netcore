using System.Net;
using System.Text;
using System.Web;
using File = System.IO.File;

namespace talentconsulting.open_referral_client.tests
{
    public partial class Serialises_extra_properties
    {
        public class MockHttpMessageHandler : HttpMessageHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                switch (request.RequestUri.LocalPath.Split("/").Last())
                {
                    case "services":

                        var page = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("page");

                        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            RequestMessage = request
                        };

                        switch (page)
                        {
                            case "2":
                                responseMessage.Content = new StringContent(File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                    "canned-responses/services_page_2.json").ReplaceLineEndings()).Result, Encoding.UTF8, "application/json");
                                break;
                            default:
                                responseMessage.Content = new StringContent(File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                    "canned-responses/services_page_1.json").ReplaceLineEndings()).Result, Encoding.UTF8, "application/json");
                                break;
                        }

                        return await Task.FromResult(responseMessage);
                    default:
                        var localpath = request.RequestUri.LocalPath.Split("/");

                        if (localpath.SkipLast(1).Last() == "services")
                        {
                            //we have an individual service request
                            if (localpath.Last() == "unknown")
                            {
                                responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
                                {
                                    RequestMessage = request
                                };
                            }
                            else
                            {
                                responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                                {
                                    RequestMessage = request
                                };

                                responseMessage.Content = new StringContent(File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                    "canned-responses/service_response.json").ReplaceLineEndings()).Result, Encoding.UTF8, "application/json");
                            }

                            return await Task.FromResult(responseMessage);
                        }

                        break;
                }

                throw new NotImplementedException("Interface not mocked");
            }
        }
    }
}