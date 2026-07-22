using System;
using System.Collections.Generic;
using System.Text;

namespace WorkFit.ProjectManagement.Features.Project.CreateProject;
public sealed record RequiredSkillInputDto(
    Guid SkillId,
    string Level,
    int Priority);