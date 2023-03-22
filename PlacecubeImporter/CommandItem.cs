using FamilyHubs.ServiceDirectory.Shared.Dto;

namespace PlacecubeImporter;

public class CommandItem
{
    public string Name { get; set; }
    public string BaseUrl { get; set; }
    public string AdminAreaCode { get; set; }
    public OrganisationWithServicesDto ParentOrganisation { get; set; }
}
