// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class TeacherFreeDateModel : BaseModel
    {
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public Guid TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public string? TeacherCode { get; set; }
        public IList<TeacherFreeTimeModel>? TeacherFreeTimes { get; set; }
    }
}
