using WorkFit.TalentManagement.Domain.Entities;

namespace WorkFit.TalentManagement.Application.Abstractions;

public interface IEmployeeRepository
{
    // --- Read ---
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // بيجيب الموظف مع كل التفاصيل (skills, certs, domains)
    Task<Employee?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<List<Employee>> GetAllAsync(Guid orgId, CancellationToken ct = default);

    // بحث نصي + فلترة بالمهارات
    Task<List<Employee>> SearchAsync(string? text, List<string>? skills,
        CancellationToken ct = default);

    // الموظفين اللي عندهم availability > 0% (فاضيين جزئياً أو كلياً)
    Task<List<Employee>> GetBenchAsync(Guid orgId, CancellationToken ct = default);

    // --- Write ---
    Task AddAsync(Employee employee, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}