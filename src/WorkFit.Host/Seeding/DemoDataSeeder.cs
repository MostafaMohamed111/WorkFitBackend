using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkFit.Identity.Domain.Entities;
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.Host.Seeding;

internal static class DemoDataSeeder
{
    public static async Task SeedDemoDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<WorkFitUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<WorkFitRole>>();
        var orgDb = scope.ServiceProvider.GetRequiredService<OrganizationDbContext>();
        var talentDb = scope.ServiceProvider.GetRequiredService<TalentDbContext>();

        // 1. Ensure Roles
        string[] roleNames = { "SuperAdmin", "Admin", "OrganizationOwner", "Employee", "TeamLeader" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new WorkFitRole(roleName));
            }
        }

        const string password = "Karim@123";

        // 2. Primary Owner User & Organization setup
        var primaryOwnerEmail = "owner@teamleader.com";
        var primaryOwnerUser = await userManager.FindByEmailAsync(primaryOwnerEmail);
        if (primaryOwnerUser is null)
        {
            primaryOwnerUser = new WorkFitUser(primaryOwnerEmail, "Organization Owner");
            var result = await userManager.CreateAsync(primaryOwnerUser, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(primaryOwnerUser, "OrganizationOwner");
            }
        }
        else
        {
            primaryOwnerUser.PasswordHash = userManager.PasswordHasher.HashPassword(primaryOwnerUser, password);
            await userManager.UpdateAsync(primaryOwnerUser);
            if (!await userManager.IsInRoleAsync(primaryOwnerUser, "OrganizationOwner"))
            {
                await userManager.AddToRoleAsync(primaryOwnerUser, "OrganizationOwner");
            }
        }

        var org = await orgDb.Organizations.FirstOrDefaultAsync(x => x.UserId == primaryOwnerUser!.Id);
        if (org is null)
        {
            org = Organization.Create("WorkFit Demo Organization", primaryOwnerUser!.Id);
            orgDb.Organizations.Add(org);
            await orgDb.SaveChangesAsync();
        }

        // 3. Seed Owner Email Aliases
        var ownerEmails = new[] { "owner@teamleader.com", "owner@owner.com", "Owner@Owner.com" };
        foreach (var email in ownerEmails)
        {
            await EnsureUserWithRoleAsync(
                userManager,
                talentDb,
                org.Id,
                email,
                "Organization Owner",
                "Organization Owner",
                "Organization Owner Bio",
                [],
                "OrganizationOwner",
                password);
        }

        // 4. Seed Team Leader Email Aliases
        var teamLeaderEmails = new[] { "teamleader@teamleader.com", "karim@teamleader.com", "Karim@teamleader.com" };
        foreach (var email in teamLeaderEmails)
        {
            await EnsureUserWithRoleAsync(
                userManager,
                talentDb,
                org.Id,
                email,
                "Team Leader",
                "Team Lead",
                "Team Leader managing projects and employee recommendations",
                [],
                "TeamLeader",
                password);
        }

        // 5. Seed 5 Developers with Distinct Skill Sets for Testing AI Recommendation
        var testEmployees = new[]
        {
            (
                "dev1@teamleader.com",
                "John Angular",
                "Senior Frontend Engineer",
                "Expert Frontend Developer specializing in Angular 19, TypeScript, RxJS, NgRx, and web components.",
                new[] { ("Angular", 95), ("TypeScript", 90), ("RxJS", 88), ("Tailwind CSS", 85), ("HTML5", 92) }
            ),
            (
                "dev2@teamleader.com",
                "Sarah Dotnet",
                "Backend .NET Architect",
                "Architect specializing in ASP.NET Core, C#, EF Core, SQL Server, Clean Architecture, and REST APIs.",
                new[] { ("C#", 95), ("ASP.NET Core", 95), ("EF Core", 90), ("SQL Server", 90), ("Clean Architecture", 92) }
            ),
            (
                "dev3@teamleader.com",
                "Michael Cloud",
                "Senior DevOps & Cloud Engineer",
                "Specialist in Docker, Kubernetes, CI/CD Pipelines, Azure deployment, and Infrastructure as Code.",
                new[] { ("Docker", 95), ("Kubernetes", 90), ("CI/CD Pipelines", 92), ("Azure", 88), ("Terraform", 85) }
            ),
            (
                "dev4@teamleader.com",
                "Emily Davis",
                "UI/UX Developer & Designer",
                "Senior UI/UX Developer and Designer specialized in UI/UX Design, Figma, Wireframing, Prototyping, User Research, Design Systems, and HTML/CSS.",
                new[] { ("UI/UX Design", 95), ("Figma", 95), ("Wireframing", 90), ("Prototyping", 92), ("User Research", 88), ("Design Systems", 90) }
            ),
            (
                "dev5@teamleader.com",
                "Alex Test",
                "Lead QA Automation Engineer",
                "QA Automation expert specialized in Playwright, Cypress, Selenium, End-to-End Testing, and API testing.",
                new[] { ("Playwright", 95), ("Cypress", 90), ("Selenium", 88), ("QA Automation", 92), ("API Testing", 90) }
            )
        };

        foreach (var (empEmail, empName, empTitle, empBio, empSkills) in testEmployees)
        {
            await EnsureUserWithRoleAsync(
                userManager,
                talentDb,
                org.Id,
                empEmail,
                empName,
                empTitle,
                empBio,
                empSkills,
                "Employee",
                password);
        }
    }

    private static async Task EnsureUserWithRoleAsync(
        UserManager<WorkFitUser> userManager,
        TalentDbContext talentDb,
        Guid orgId,
        string email,
        string name,
        string title,
        string bio,
        (string name, int score)[] skills,
        string roleName,
        string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new WorkFitUser(email, name);
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, roleName);
            }
        }
        else
        {
            user.PasswordHash = userManager.PasswordHasher.HashPassword(user, password);
            await userManager.UpdateAsync(user);
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
            }
        }

        if (user is not null)
        {
            var profile = await talentDb.EmployeeProfiles
                .Include(p => p.EmployeeSkills)
                .FirstOrDefaultAsync(x => x.UserId == user.Id || x.Email == email);

            if (profile is null)
            {
                profile = EmployeeProfile.Create(orgId, user.Id, email, name, title);
                profile.ActivateEmployee();
                talentDb.EmployeeProfiles.Add(profile);
            }
            else
            {
                var entry = talentDb.Entry(profile);
                entry.Property("OrganizationId").CurrentValue = orgId;
                profile.ActivateEmployee();
            }

            var nameProp = typeof(EmployeeProfile).GetProperty(nameof(EmployeeProfile.Name));
            nameProp?.SetValue(profile, name);

            var titleProp = typeof(EmployeeProfile).GetProperty(nameof(EmployeeProfile.JobTitle));
            titleProp?.SetValue(profile, title);

            var bioProp = typeof(EmployeeProfile).GetProperty(nameof(EmployeeProfile.Bio));
            bioProp?.SetValue(profile, bio);

            if (skills.Length > 0)
            {
                foreach (var (skillName, score) in skills)
                {
                    var existingSkill = profile.EmployeeSkills.FirstOrDefault(s => string.Equals(s.SkillName, skillName, StringComparison.OrdinalIgnoreCase));
                    if (existingSkill is null)
                    {
                        profile.AddOrUpdateEmployeeSkill(
                            Guid.NewGuid(),
                            Guid.NewGuid(),
                            skillName,
                            score,
                            $"Verified skill in {skillName}",
                            "System Seed");
                    }
                    else
                    {
                        profile.AddOrUpdateEmployeeSkill(
                            existingSkill.SkillId,
                            existingSkill.Id,
                            skillName,
                            score,
                            $"Updated skill in {skillName}",
                            "System Seed");
                    }
                }
            }

            try
            {
                await talentDb.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Concurrency safe
            }
        }
    }
}
