using FamilyHubs.ServiceDirectory.Shared.Dto;

namespace PluginBase;

public record MappedOrganisation
{
    public MappedOrganisation(OrganisationWithServicesDto organisationWithServicesDto, bool isNew)
    {
        OrganisationWithServicesDto = organisationWithServicesDto;
        IsNew = isNew;
    }
    public bool IsNew { get; init; }
    public OrganisationWithServicesDto OrganisationWithServicesDto { get; init; }
}

