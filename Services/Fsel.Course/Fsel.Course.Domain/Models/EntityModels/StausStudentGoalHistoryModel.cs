// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Core.Base.BaseModels;
    using Shared.Enums;

    public class StausStudentGoalHistoryModel : BaseModel
    {
        public EnumStatusStudentGoal StatusStudentGoal { get; set; }
        public Guid StudentId { get; set; }
    }
}
