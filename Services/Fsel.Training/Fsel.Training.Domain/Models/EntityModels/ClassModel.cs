// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Domain.Entities;
    using System.ComponentModel.DataAnnotations;

    public class ClassModel : BaseModel
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime StartTime { get; set; }

        public EnumClassType Status { get; set; }

        public Guid CourseId { get; set; }

        public Guid PackageId { get; set; }

        public DayOfWeek LiveDays { get; set; }

        public Guid? TeacherId { get; set; }

        public Guid? CsoId { get; set; }

        public IList<ClassStudentModel>? ClassStudents { get; set; }
        public IList<ClassLiveCalendarModel>? ClassLiveCalendars { get; set; }
    }
}
