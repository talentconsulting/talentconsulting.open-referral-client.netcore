// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using talentconsulting.open_referral_client;

var client = new OpenReferralClient(new Uri("https://api.familyinfo.buckinghamshire.gov.uk"), "api/v1");

var services = await client.GetServices(new
{
});




