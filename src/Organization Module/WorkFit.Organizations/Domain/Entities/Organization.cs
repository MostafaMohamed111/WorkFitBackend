using WorkFit.SharedKernel.BaseEntity;
using WorkFit.Organizations.Domain.Exceptions;

namespace WorkFit.Organizations.Domain.Entities;

public sealed class Organization : BaseEntity
{
    public string Name { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public string BrandingJson { get; private set; } = "{}";
    public string SettingsJson { get; private set; } = "{}";

    private Organization() : base() { } // EF
    private Organization(string name, Guid userId, string brandingJson, string settingsJson) : base()
    {
        Name = name;
        UserId = userId;
        BrandingJson = brandingJson;
        SettingsJson = settingsJson;
    }

    public static Organization Create(string name, Guid userId)
    {
        if(string.IsNullOrEmpty(name)) throw new OrganizationNameIsNullOrEmptyException();
        return new Organization(name, userId, "{}", "{}");
    }

    public void UpdateDetails(string name, string brandingJson, string settingsJson)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new OrganizationNameIsNullOrEmptyException();

        Name = name;
        BrandingJson = string.IsNullOrWhiteSpace(brandingJson) ? "{}" : brandingJson;
        SettingsJson = string.IsNullOrWhiteSpace(settingsJson) ? "{}" : settingsJson;
        MarkUpdated();
    }

    public void UpdateSettings(string settingsJson)
    {
        SettingsJson = string.IsNullOrWhiteSpace(settingsJson) ? "{}" : settingsJson;
        MarkUpdated();
    }
}
