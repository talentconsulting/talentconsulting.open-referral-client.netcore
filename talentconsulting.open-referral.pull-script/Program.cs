// See https://aka.ms/new-console-template for more information

using talentconsulting.open_referral_client;

//IOrganisationClientService orgClient = new OrganisationClientService("https://localhost:7022/");

//List<OrganisationDto>  orgs = await orgClient.GetListOrganisations();

//var result = await orgClient.GetOrganisationById("ca8ddaeb-b5e5-46c4-b94d-43a8e2ccc066");

var client = new OpenReferralClient(new Uri("https://api.familyinfo.buckinghamshire.gov.uk"), "api/v1");

var services = await client.GetServices(new
{
});




