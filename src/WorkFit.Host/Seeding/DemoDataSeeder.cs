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

        // 3. Seed Owner Email Aliases (owner@teamleader.com, owner@owner.com, Owner@Owner.com)
        var ownerEmails = new[] { "owner@teamleader.com", "owner@owner.com", "Owner@Owner.com" };
        foreach (var email in ownerEmails)
        {
            await EnsureUserWithRoleAsync(userManager, talentDb, org.Id, email, "Organization Owner", "Organization Owner", "OrganizationOwner", password);
        }

        // 4. Seed Team Leader Email Aliases (teamleader@teamleader.com, karim@teamleader.com, Karim@teamleader.com)
        var teamLeaderEmails = new[] { "teamleader@teamleader.com", "karim@teamleader.com", "Karim@teamleader.com" };
        foreach (var email in teamLeaderEmails)
        {
            await EnsureUserWithRoleAsync(userManager, talentDb, org.Id, email, "Team Leader", "Team Lead", "TeamLeader", password);
        }

        // 5. Seed Test Organization Developers
        var testEmployees = new[]
        {
            ("dev1@teamleader.com", "John Developer", "Senior Frontend Engineer"),
            ("dev2@teamleader.com", "Sarah Jenkins", "Backend .NET Developer"),
            ("dev3@teamleader.com", "Michael Smith", "Full Stack Engineer"),
            ("dev4@teamleader.com", "Emily Davis", "UI/UX Developer"),
            ("dev5@teamleader.com", "Alex Johnson", "DevOps Specialist")
        };

        foreach (var (empEmail, empName, empTitle) in testEmployees)
        {
            await EnsureUserWithRoleAsync(userManager, talentDb, org.Id, empEmail, empName, empTitle, "Employee", password);
        }
    }

    private static async Task EnsureUserWithRoleAsync(
        UserManager<WorkFitUser> userManager,
        TalentDbContext talentDb,
        Guid orgId,
        string email,
        string name,
        string title,
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
            var profile = await talentDb.EmployeeProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id || x.Email == email);
            if (profile is null)
            {
                profile = EmployeeProfile.Create(orgId, user.Id, email, name, title);
                profile.ActivateEmployee();
                talentDb.EmployeeProfiles.Add(profile);
                await talentDb.SaveChangesAsync();
            }
            else
            {
                var entry = talentDb.Entry(profile);
                entry.Property("OrganizationId").CurrentValue = orgId;
                profile.ActivateEmployee();
                await talentDb.SaveChangesAsync();
            }
        }
    }
}
