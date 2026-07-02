using Microsoft.EntityFrameworkCore;
using WorkFit.TalentManagement.Application.Abstractions;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly TalentDbContext _db;

    public EmployeeRepository(TalentDbContext db) => _db = db;

    // Lookup بسيط بالـ ID — بيجيب معاه الـ Profile
    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Employees
            .Include(e => e.Profile)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    // Lookup كامل — بيتستخدم لما نبني صفحة الـ Talent Profile
    public async Task<Employee?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await _db.Employees
            .Include(e => e.Profile)
            .Include(e => e.Skills).ThenInclude(s => s.Evidences)
            .Include(e => e.Certifications)
            .Include(e => e.PreferredDomains.OrderBy(d => d.Order))
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    // كل الموظفين النشطين في organization معينة
    public async Task<List<Employee>> GetAllAsync(Guid orgId, CancellationToken ct = default)
        => await _db.Employees
            .Where(e => e.OrganizationId == orgId && e.IsActive)
            .OrderBy(e => e.LastName)
            .ToListAsync(ct);

    // بحث بالاسم/المسمى الوظيفي أو فلترة بالمهارات
    public async Task<List<Employee>> SearchAsync(
        string? text, List<string>? skills, CancellationToken ct = default)
    {
        var query = _db.Employees
            .Include(e => e.Skills)
            .Where(e => e.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(text))
            query = query.Where(e =>
                e.FirstName.Contains(text) ||
                e.LastName.Contains(text) ||
                e.JobTitle.Contains(text));

        if (skills != null && skills.Count > 0)
            query = query.Where(e =>
                e.Skills.Any(s => skills.Contains(s.SkillName)));

        return await query.ToListAsync(ct);
    }

    // Bench: الموظفين اللي عندهم availability
    public async Task<List<Employee>> GetBenchAsync(Guid orgId, CancellationToken ct = default)
        => await _db.Employees
            .Include(e => e.Profile)
            .Where(e => e.OrganizationId == orgId
                && e.IsActive
                && e.Profile.AvailabilityPercentage > 0)
            .ToListAsync(ct);

    public async Task AddAsync(Employee employee, CancellationToken ct = default)
        => await _db.Employees.AddAsync(employee, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}