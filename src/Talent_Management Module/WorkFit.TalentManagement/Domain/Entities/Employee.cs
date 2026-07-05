using System;
using System.Collections.Generic;
using System.Text;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities
{
    public class Employee : BaseEntity
    {
       
        // Cross-module references (IDs only)
        public Guid OrganizationId { get; private set; }
        public Guid DepartmentId { get; private set; }
        public Guid UserId { get; private set; }

        public string FirstName { get; private set; } = default!;
        public string LastName { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string JobTitle { get; private set; } = default!;
        public DateTime HireDate { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Navigation within this module only
        public TalentProfile Profile { get; private set; } = default!;
        public ICollection<EmployeeSkill> Skills { get; private set; } = [];
        public ICollection<Certification> Certifications { get; private set; } = [];
        public ICollection<PreferredDomain> PreferredDomains { get; private set; } = [];
        public ICollection<WorkHistory> WorkHistory { get; private set; } = [];

        public static Employee Create(
            Guid orgId, Guid deptId, Guid userId,
            string firstName, string lastName,
            string email, string jobTitle, DateTime hireDate) => new()
            {
                OrganizationId = orgId,
                DepartmentId = deptId,
                UserId = userId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                JobTitle = jobTitle,
                HireDate = hireDate
            };

        public void Deactivate() => IsActive = false;

        public void UpdateDetails(string firstName, string lastName, string jobTitle)
        {
            FirstName = firstName;
            LastName = lastName;
            JobTitle = jobTitle;
        }
    }
}
