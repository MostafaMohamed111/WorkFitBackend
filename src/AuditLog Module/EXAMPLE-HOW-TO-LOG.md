# How to Log Create / Update / Delete Operations

Once your team approves what to log, follow this pattern in any module handler.

## Step 1: Add project reference

Add a reference to `WorkFit.AuditLog.Contracts` in your module's `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\AuditLog Module\WorkFit.AuditLog.Contracts\WorkFit.AuditLog.Contracts.csproj" />
</ItemGroup>
```

## Step 2: Inject `IAuditLogService`

```csharp
using WorkFit.AuditLog.Contracts.Services;

public sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentResponse>
{
    private readonly OrganizationDbContext _context;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentUserContext _currentUser;

    public CreateDepartmentCommandHandler(
        OrganizationDbContext context,
        IAuditLogService auditLog,
        ICurrentUserContext currentUser)
    {
        _context = context;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }
}
```

## Step 3: Call the service after `SaveChangesAsync`

```csharp
public async Task<DepartmentResponse> Handle(CreateDepartmentCommand request, CancellationToken ct)
{
    var department = Department.Create(request.Name, request.OrganizationId);
    _context.Departments.Add(department);
    await _context.SaveChangesAsync(ct);

    await _auditLog.LogCreatedAsync(
        department.Id,
        nameof(Department),
        _currentUser.GetUserId(),
        _currentUser.GetUserDisplayName(),
        ModuleMarker.ModuleName,
        ct
    );

    return new DepartmentResponse(department.Id, department.OrganizationId, department.Name, department.CreatedAt, department.UpdatedAt);
}
```

## Available methods

| Method | Logs action | Use after |
|--------|-------------|-----------|
| `LogCreatedAsync(...)` | "Created" | Entity is created |
| `LogUpdatedAsync(...)` | "Updated" | Entity is updated |
| `LogDeletedAsync(...)` | "Deleted" | Entity is deleted |

## Parameters

| Parameter | Source | Example |
|-----------|--------|---------|
| `entityId` | `entity.Id` | `department.Id` |
| `entityType` | `nameof(EntityClass)` | `"Department"` |
| `userId` | `_currentUser.GetUserId()` | `Guid` |
| `userDisplayName` | `_currentUser.GetUserDisplayName()` | `"Ahmed Hassan"` |
| `moduleName` | `ModuleMarker.ModuleName` | `"Organization_Module"` |

## What gets stored

All fields go into the `audit_logs` table in the `AuditLog` schema — one row per action.
