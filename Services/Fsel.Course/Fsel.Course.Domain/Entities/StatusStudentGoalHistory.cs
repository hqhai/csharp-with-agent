// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Core.Entities;
    using Shared.Enums;

    public class StatusStudentGoalHistory : Entity
    {
        public Guid StudentId { get; set; }
        public EnumStatusStudentGoal StatusStudentGoal  { get; set; }
    }
}
