using FamilyHubs.ServiceDirectory.Shared.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PluginBase
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }

        Task<int> Execute();
    }
}
