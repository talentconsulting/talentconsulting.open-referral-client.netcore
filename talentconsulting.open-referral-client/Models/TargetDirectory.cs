using System;
namespace talentconsulting.open_referral_client.Models
{
    public class TargetDirectory : BaseModel
    {
		public string Name { get; set; }

        public string Label { get; set; }
    }
}


//{
//    "id": 1784,
//          "name": "",
//          "address_1": null,
//          "city": "Amersham",
//          "state_province": "Buckinghamshire",
//          "postal_code": "HP7",
//          "country": "GB",
//          "geometry": {
//        "type": "Point",
//            "coordinates": [
//              -0.6,
//              51.67
//            ]
//          },
//          "mask_exact_address": true,
//          "accessibilities": [
//            {
//        "name": "Accessible toilet facilities",
//              "slug": "accessible-toilet-facilities"
//            },
//            {
//        "name": "Car parking",
//              "slug": "car-parking"
//            },
//            {
//        "name": "Bus stop nearby",
//              "slug": "bus-stop-nearby"
//            }
//          ]
//        }