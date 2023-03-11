using FamilyHubs.ServiceDirectory.Shared.Dto;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelloPlugin
{
    public class HelloCommand : ICommand
    {
        public string Name { get => "hello"; }
        public string Description { get => "Displays hello message."; }

        public async Task<int> Execute()
        {
            Console.WriteLine("Hello !!!");
            BuckinghamshireMapper BuckinghamshireMapper = new BuckinghamshireMapper();
            await BuckinghamshireMapper.AddOrUpdateServices("https://api.familyinfo.buckinghamshire.gov.uk", "api/v1");
            return 0;
        }

        //public int ExecuteWithDependency(List<OrganisationDto> organisations)
        //{
            
        //    return 0;
        //}

        //public async Task<List<MappedOrganisation>> ExecuteWithDependency(Dictionary<string, OrganisationDto> dictOrganisations)
        //{
        //    Console.WriteLine("Hello !!!");
        //    return new List<MappedOrganisation>();
        //    //BuckinghamshireMapper BuckinghamshireMapper = new BuckinghamshireMapper(dictOrganisations);
        //    //return 
        //}
    }
}
