//using System.Reflection;
//using Microsoft.EntityFrameworkCore;
//using WorkFit.ProjectManagement.Domain.Entities;
//using WorkFit.ProjectManagement.Domain.Enums;
//using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

//namespace WorkFit.ProjectManagement.Infrastructure.Seed;

///// <summary>
///// Seeds baseline demo data for the Project Management module.
/////
///// IMPORTANT — cross-module references:
///// Department, Employee, Skill and Domain all live in OTHER bounded-context
///// modules (Organization, Identity/Talent, Talent Management) that have not
///// been implemented yet. Every id below (DepartmentId, EmployeeId, SkillId,
///// DomainId, ActorId, CreatedById, AssigneeId) is therefore a FAKE, FIXED
///// GUID acting as a stand-in FK. Nothing is inserted into other modules'
///// tables — these are just plausible-looking foreign keys so this module
///// can be seeded and exercised in isolation.
/////
///// Once the real modules exist, delete the "Fake cross-module reference
///// data" region and swap in real lookups (e.g. via a shared reference
///// service, or a proper cross-module seed order).
///// </summary>
//public static class ProjectManagementSeeder
//{
//    #region Fake cross-module reference data (replace once real modules exist)

//    // ---- Organization module (not built yet) — fake Department ids ----
//    private static readonly Guid Dept_Engineering = Guid.Parse("11111111-0000-0000-0000-000000000001");
//    private static readonly Guid Dept_Product = Guid.Parse("11111111-0000-0000-0000-000000000002");
//    private static readonly Guid Dept_DataScience = Guid.Parse("11111111-0000-0000-0000-000000000003");

//    // ---- Organization module (not built yet) — fake business Domain ids ----
//    private static readonly Guid Domain_Fintech = Guid.Parse("22222222-0000-0000-0000-000000000001");
//    private static readonly Guid Domain_Healthcare = Guid.Parse("22222222-0000-0000-0000-000000000002");
//    private static readonly Guid Domain_InternalIT = Guid.Parse("22222222-0000-0000-0000-000000000003");

//    // ---- Identity/Talent module (not built yet) — fake Employee ids ----
//    private static readonly Guid Emp_Alice = Guid.Parse("33333333-0000-0000-0000-000000000001"); // PM
//    private static readonly Guid Emp_Bob = Guid.Parse("33333333-0000-0000-0000-000000000002"); // Backend dev
//    private static readonly Guid Emp_Carol = Guid.Parse("33333333-0000-0000-0000-000000000003"); // Frontend dev
//    private static readonly Guid Emp_Dave = Guid.Parse("33333333-0000-0000-0000-000000000004"); // Designer
//    private static readonly Guid Emp_Erin = Guid.Parse("33333333-0000-0000-0000-000000000005"); // Data engineer
//    private static readonly Guid Emp_Frank = Guid.Parse("33333333-0000-0000-0000-000000000006"); // QA

//    // ---- Talent Management module (not built yet) — fake Skill ids ----
//    private static readonly Guid Skill_CSharp = Guid.Parse("44444444-0000-0000-0000-000000000001");
//    private static readonly Guid Skill_React = Guid.Parse("44444444-0000-0000-0000-000000000002");
//    private static readonly Guid Skill_SQL = Guid.Parse("44444444-0000-0000-0000-000000000003");
//    private static readonly Guid Skill_MachineLearning = Guid.Parse("44444444-0000-0000-0000-000000000004");
//    private static readonly Guid Skill_UXDesign = Guid.Parse("44444444-0000-0000-0000-000000000005");

//    #endregion

//    /// <summary>
//    /// Seeds Projects, ProjectTasks, ProjectAssignments, ProjectRequiredSkills,
//    /// ProjectDomains and an initial ProjectActivityLog entry for each project.
//    /// Safe to call on every startup — it no-ops if projects already exist.
//    /// </summary>
//    public static async Task SeedAsync(DbContext context, CancellationToken ct = default)
//    {
//        if (await context.Set<Project>().AnyAsync(ct))
//        {
//            return; // already seeded
//        }

//        var projects = BuildProjects();

//        await context.Set<Project>().AddRangeAsync(projects, ct);
//        await context.SaveChangesAsync(ct);
//    }

//    private static List<Project> BuildProjects()
//    {
//        var now = DateTimeOffset.UtcNow;

//        // ---------------- Project 1: Customer Portal Redesign ----------------
//        var project1 = NewEntity<Project>(new()
//        {
//            ["DepartmentId"] = Dept_Product,
//            ["Name"] = "Customer Portal Redesign",
//            ["Description"] = "Revamp of the self-service customer portal with a new UX and faster load times.",
//            ["Status"] = ProjectStatus.Planning,
//            ["StartDate"] = DateOnly.FromDateTime(now.AddDays(14).Date),
//            ["EndDate"] = DateOnly.FromDateTime(now.AddMonths(4).Date),
//        });

//        SetProperty(project1, "Tasks", new List<ProjectTask>
//        {
//            NewEntity<ProjectTask>(new()
//            {
//                ["ProjectId"] = project1.Id,
//                ["Title"] = "Design new dashboard wireframes",
//                ["Description"] = "Low-fidelity wireframes for the redesigned customer dashboard.",
//                ["TaskType"] = TaskType.Task,
//                ["Status"] = TaskStatus.InProgress,
//                ["Priority"] = TaskPriority.High,
//                ["AssigneeId"] = Emp_Dave,
//                ["CreatedById"] = Emp_Alice,
//                ["StoryPoints"] = 5,
//                ["DueDate"] = DateOnly.FromDateTime(now.AddDays(21).Date),
//                ["SourceSystem"] = "internal",
//            }),
//            NewEntity<ProjectTask>(new()
//            {
//                ["ProjectId"] = project1.Id,
//                ["Title"] = "Set up portal frontend scaffolding",
//                ["Description"] = "Bootstrap the new React app with routing and design tokens.",
//                ["TaskType"] = TaskType.Story,
//                ["Status"] = TaskStatus.ToDo,
//                ["Priority"] = TaskPriority.Medium,
//                ["AssigneeId"] = Emp_Carol,
//                ["CreatedById"] = Emp_Alice,
//                ["StoryPoints"] = 8,
//                ["DueDate"] = DateOnly.FromDateTime(now.AddDays(30).Date),
//                ["SourceSystem"] = "jira",
//                ["SourceReferenceId"] = "PORTAL-101",
//            }),
//        });

//        SetProperty(project1, "Assignments", new List<ProjectAssignment>
//        {
//            NewEntity<ProjectAssignment>(new()
//            {
//                ["ProjectId"] = project1.Id,
//                ["EmployeeId"] = Emp_Alice,
//                ["RoleOnProject"] = "Project Manager",
//                ["AllocationPercentage"] = 30,
//                ["StartDate"] = DateOnly.FromDateTime(now.Date),
//                ["IsActive"] = true,
//                ["JoinedAt"] = now,
//            }),
//            NewEntity<ProjectAssignment>(new()
//            {
//                ["ProjectId"] = project1.Id,
//                ["EmployeeId"] = Emp_Dave,
//                ["RoleOnProject"] = "UX Designer",
//                ["AllocationPercentage"] = 50,
//                ["StartDate"] = DateOnly.FromDateTime(now.Date),
//                ["IsActive"] = true,
//                ["JoinedAt"] = now,
//            }),
//        });

//        SetProperty(project1, "RequiredSkills", new List<ProjectRequiredSkill>
//        {
//            NewEntity<ProjectRequiredSkill>(new()
//            {
//                ["ProjectId"] = project1.Id,
//                ["SkillId"] = Skill_UXDesign,
//                ["Level"] = SkillLevel.Expert,
//                ["Priority"] = 1,
//            }),
//            NewEntity<ProjectRequiredSkill>(new()
//            {
//                ["ProjectId"] = project1.Id,
//                ["SkillId"] = Skill_React,
//                ["Level"] = SkillLevel.Proficient,
//                ["Priority"] = 2,
//            }),
//        });

//        SetProperty(project1, "Domains", new List<ProjectDomain>
//        {
//            NewEntity<ProjectDomain>(new()
//            {
//                ["ProjectId"] = project1.Id,
//                ["DomainId"] = Domain_InternalIT,
//            }),
//        });

//        SetProperty(project1, "ActivityLogs", new List<ProjectActivityLog>
//        {
//            NewEntity<ProjectActivityLog>(new()
//            {
//                ["ProjectId"] = project1.Id,
//                ["ActorId"] = Emp_Alice,
//                ["Action"] = ActivityActions.ProjectCreated,
//                ["EntityType"] = ActivityEntityType.Project,
//                ["EntityId"] = project1.Id,
//                ["AfterState"] = "{\"status\":\"planning\"}",
//                ["CreatedAt"] = now,
//            }),
//        });

//        // ---------------- Project 2: Payment Gateway Integration ----------------
//        var project2 = NewEntity<Project>(new()
//        {
//            ["DepartmentId"] = Dept_Engineering,
//            ["Name"] = "Payment Gateway Integration",
//            ["Description"] = "Integrate a new PCI-compliant payment gateway into the checkout flow.",
//            ["Status"] = ProjectStatus.Active,
//            ["StartDate"] = DateOnly.FromDateTime(now.AddMonths(-1).Date),
//            ["EndDate"] = DateOnly.FromDateTime(now.AddMonths(2).Date),
//        });

//        SetProperty(project2, "Tasks", new List<ProjectTask>
//        {
//            NewEntity<ProjectTask>(new()
//            {
//                ["ProjectId"] = project2.Id,
//                ["Title"] = "Implement gateway webhook handler",
//                ["Description"] = "Handle async payment confirmation callbacks securely.",
//                ["TaskType"] = TaskType.Story,
//                ["Status"] = TaskStatus.InProgress,
//                ["Priority"] = TaskPriority.Critical,
//                ["AssigneeId"] = Emp_Bob,
//                ["CreatedById"] = Emp_Alice,
//                ["StoryPoints"] = 13,
//                ["DueDate"] = DateOnly.FromDateTime(now.AddDays(10).Date),
//                ["SourceSystem"] = "azuredevops",
//                ["SourceReferenceId"] = "PAY-207",
//            }),
//            NewEntity<ProjectTask>(new()
//            {
//                ["ProjectId"] = project2.Id,
//                ["Title"] = "Fix currency rounding bug in refunds",
//                ["Description"] = "Refund amounts are rounding incorrectly for JPY transactions.",
//                ["TaskType"] = TaskType.Bug,
//                ["Status"] = TaskStatus.Review,
//                ["Priority"] = TaskPriority.High,
//                ["AssigneeId"] = Emp_Bob,
//                ["CreatedById"] = Emp_Frank,
//                ["StoryPoints"] = 3,
//                ["DueDate"] = DateOnly.FromDateTime(now.AddDays(5).Date),
//                ["SourceSystem"] = "azuredevops",
//                ["SourceReferenceId"] = "PAY-214",
//            }),
//            NewEntity<ProjectTask>(new()
//            {
//                ["ProjectId"] = project2.Id,
//                ["Title"] = "QA sign-off on sandbox transactions",
//                ["Description"] = "Run full regression suite against gateway sandbox.",
//                ["TaskType"] = TaskType.Task,
//                ["Status"] = TaskStatus.Done,
//                ["Priority"] = TaskPriority.Medium,
//                ["AssigneeId"] = Emp_Frank,
//                ["CreatedById"] = Emp_Alice,
//                ["StoryPoints"] = 5,
//                ["DueDate"] = DateOnly.FromDateTime(now.AddDays(-2).Date),
//                ["SourceSystem"] = "internal",
//                ["CompletedAt"] = now.AddDays(-3),
//            }),
//        });

//        SetProperty(project2, "Assignments", new List<ProjectAssignment>
//        {
//            NewEntity<ProjectAssignment>(new()
//            {
//                ["ProjectId"] = project2.Id,
//                ["EmployeeId"] = Emp_Bob,
//                ["RoleOnProject"] = "Lead Backend Engineer",
//                ["AllocationPercentage"] = 80,
//                ["StartDate"] = DateOnly.FromDateTime(now.AddMonths(-1).Date),
//                ["IsActive"] = true,
//                ["JoinedAt"] = now.AddMonths(-1),
//            }),
//            NewEntity<ProjectAssignment>(new()
//            {
//                ["ProjectId"] = project2.Id,
//                ["EmployeeId"] = Emp_Frank,
//                ["RoleOnProject"] = "QA Engineer",
//                ["AllocationPercentage"] = 40,
//                ["StartDate"] = DateOnly.FromDateTime(now.AddMonths(-1).Date),
//                ["IsActive"] = true,
//                ["JoinedAt"] = now.AddMonths(-1),
//            }),
//        });

//        SetProperty(project2, "RequiredSkills", new List<ProjectRequiredSkill>
//        {
//            NewEntity<ProjectRequiredSkill>(new()
//            {
//                ["ProjectId"] = project2.Id,
//                ["SkillId"] = Skill_CSharp,
//                ["Level"] = SkillLevel.Expert,
//                ["Priority"] = 1,
//            }),
//            NewEntity<ProjectRequiredSkill>(new()
//            {
//                ["ProjectId"] = project2.Id,
//                ["SkillId"] = Skill_SQL,
//                ["Level"] = SkillLevel.Proficient,
//                ["Priority"] = 2,
//            }),
//        });

//        SetProperty(project2, "Domains", new List<ProjectDomain>
//        {
//            NewEntity<ProjectDomain>(new()
//            {
//                ["ProjectId"] = project2.Id,
//                ["DomainId"] = Domain_Fintech,
//            }),
//        });

//        SetProperty(project2, "ActivityLogs", new List<ProjectActivityLog>
//        {
//            NewEntity<ProjectActivityLog>(new()
//            {
//                ["ProjectId"] = project2.Id,
//                ["ActorId"] = Emp_Alice,
//                ["Action"] = ActivityActions.ProjectCreated,
//                ["EntityType"] = ActivityEntityType.Project,
//                ["EntityId"] = project2.Id,
//                ["AfterState"] = "{\"status\":\"planning\"}",
//                ["CreatedAt"] = now.AddMonths(-1),
//            }),
//            NewEntity<ProjectActivityLog>(new()
//            {
//                ["ProjectId"] = project2.Id,
//                ["ActorId"] = Emp_Alice,
//                ["Action"] = ActivityActions.ProjectStatusChanged,
//                ["EntityType"] = ActivityEntityType.Project,
//                ["EntityId"] = project2.Id,
//                ["BeforeState"] = "{\"status\":\"planning\"}",
//                ["AfterState"] = "{\"status\":\"active\"}",
//                ["CreatedAt"] = now.AddDays(-25),
//            }),
//        });

//        // ---------------- Project 3: Internal HR Automation ----------------
//        var project3 = NewEntity<Project>(new()
//        {
//            ["DepartmentId"] = Dept_DataScience,
//            ["Name"] = "Internal HR Automation",
//            ["Description"] = "Automate onboarding/offboarding workflows using internal ML services.",
//            ["Status"] = ProjectStatus.OnHold,
//            ["StartDate"] = DateOnly.FromDateTime(now.AddMonths(-3).Date),
//            ["EndDate"] = null,
//        });

//        SetProperty(project3, "Tasks", new List<ProjectTask>
//        {
//            NewEntity<ProjectTask>(new()
//            {
//                ["ProjectId"] = project3.Id,
//                ["Title"] = "Prototype resume-parsing model",
//                ["Description"] = "Evaluate feasibility of automated resume field extraction.",
//                ["TaskType"] = TaskType.Epic,
//                ["Status"] = TaskStatus.ToDo,
//                ["Priority"] = TaskPriority.Low,
//                ["AssigneeId"] = Emp_Erin,
//                ["CreatedById"] = Emp_Alice,
//                ["StoryPoints"] = 20,
//                ["SourceSystem"] = "internal",
//            }),
//        });

//        SetProperty(project3, "Assignments", new List<ProjectAssignment>
//        {
//            NewEntity<ProjectAssignment>(new()
//            {
//                ["ProjectId"] = project3.Id,
//                ["EmployeeId"] = Emp_Erin,
//                ["RoleOnProject"] = "Data Engineer",
//                ["AllocationPercentage"] = 20,
//                ["StartDate"] = DateOnly.FromDateTime(now.AddMonths(-3).Date),
//                ["EndDate"] = DateOnly.FromDateTime(now.AddDays(-15).Date),
//                ["IsActive"] = false,
//                ["JoinedAt"] = now.AddMonths(-3),
//            }),
//        });

//        SetProperty(project3, "RequiredSkills", new List<ProjectRequiredSkill>
//        {
//            NewEntity<ProjectRequiredSkill>(new()
//            {
//                ["ProjectId"] = project3.Id,
//                ["SkillId"] = Skill_MachineLearning,
//                ["Level"] = SkillLevel.Proficient,
//                ["Priority"] = 1,
//            }),
//        });

//        SetProperty(project3, "Domains", new List<ProjectDomain>
//        {
//            NewEntity<ProjectDomain>(new()
//            {
//                ["ProjectId"] = project3.Id,
//                ["DomainId"] = Domain_Healthcare,
//            }),
//        });

//        SetProperty(project3, "ActivityLogs", new List<ProjectActivityLog>
//        {
//            NewEntity<ProjectActivityLog>(new()
//            {
//                ["ProjectId"] = project3.Id,
//                ["ActorId"] = Emp_Alice,
//                ["Action"] = ActivityActions.ProjectCreated,
//                ["EntityType"] = ActivityEntityType.Project,
//                ["EntityId"] = project3.Id,
//                ["AfterState"] = "{\"status\":\"planning\"}",
//                ["CreatedAt"] = now.AddMonths(-3),
//            }),
//            NewEntity<ProjectActivityLog>(new()
//            {
//                ["ProjectId"] = project3.Id,
//                ["ActorId"] = Emp_Alice,
//                ["Action"] = ActivityActions.ProjectStatusChanged,
//                ["EntityType"] = ActivityEntityType.Project,
//                ["EntityId"] = project3.Id,
//                ["BeforeState"] = "{\"status\":\"active\"}",
//                ["AfterState"] = "{\"status\":\"on_hold\"}",
//                ["CreatedAt"] = now.AddDays(-15),
//            }),
//        });

//        return new List<Project> { project1, project2, project3 };
//    }

//    #region Reflection helpers — needed because entity properties use `private set`

//    /// <summary>
//    /// Resolves a property by name, walking from the most-derived type up
//    /// through its base classes and returning the first match found at each
//    /// level via <see cref="BindingFlags.DeclaredOnly"/>.
//    ///
//    /// This is required because entities like <c>ProjectActivityLog</c>
//    /// inherit a <c>CreatedAt</c> property from <c>BaseEntity</c>. A plain
//    /// <c>Type.GetProperty(name, flags)</c> call with
//    /// <see cref="BindingFlags.NonPublic"/> set will find BOTH the base and
//    /// derived declarations and throw <see cref="AmbiguousMatchException"/>.
//    /// Walking the hierarchy manually and returning the first (most-derived)
//    /// match avoids that ambiguity entirely.
//    /// </summary>
//    private static PropertyInfo ResolveProperty(Type type, string propertyName)
//    {
//        for (var t = type; t is not null; t = t.BaseType)
//        {
//            var property = t.GetProperties(
//                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
//                    BindingFlags.DeclaredOnly)
//                .FirstOrDefault(p => p.Name == propertyName);

//            if (property is not null)
//            {
//                return property;
//            }
//        }

//        throw new InvalidOperationException(
//            $"Property '{propertyName}' not found on type '{type.Name}'.");
//    }

//    /// <summary>
//    /// Creates an instance of <typeparamref name="T"/> via its parameterless
//    /// constructor and populates the given properties (including ones with
//    /// `private set`) using reflection. This keeps the domain model's
//    /// encapsulation intact for normal application code while still allowing
//    /// seed/test data to be built without adding public setters or factory
//    /// methods purely for seeding purposes.
//    /// </summary>
//    private static T NewEntity<T>(Dictionary<string, object?> values) where T : new()
//    {
//        var entity = new T();
//        var type = typeof(T);

//        foreach (var (propertyName, value) in values)
//        {
//            var property = ResolveProperty(type, propertyName);
//            property.SetValue(entity, value);
//        }

//        return entity;
//    }

//    /// <summary>
//    /// Sets a single property (including navigation collections declared with
//    /// `private set`, such as Project.Tasks) on an already-constructed entity.
//    /// </summary>
//    private static void SetProperty<T>(T entity, string propertyName, object? value)
//    {
//        var property = ResolveProperty(typeof(T), propertyName);
//        property.SetValue(entity, value);
//    }

//    #endregion
//}