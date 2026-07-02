using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

public class TalentProfile : BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid EmployeeId { get; private set; }
    public string? Bio { get; private set; }
    public string? LinkedInUrl { get; private set; }

  
    public int AvailabilityPercentage { get; private set; } = 100;
    public bool MobilityReady { get; private set; } = false;

    public Employee Employee { get; private set; } = default!;

    public static TalentProfile Create(Guid employeeId) => new()
    {
        EmployeeId = employeeId
    };

    public void SetAvailability(int percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage));
        AvailabilityPercentage = percentage;
    }

    public void SetMobility(bool isReady) => MobilityReady = isReady;

    public void UpdateProfile(string? bio, string? linkedIn)
    {
        Bio = bio;
        LinkedInUrl = linkedIn;
    }
}