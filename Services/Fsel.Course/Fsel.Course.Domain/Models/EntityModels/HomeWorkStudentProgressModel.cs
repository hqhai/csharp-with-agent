// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;

    public class HomeWorkStudentProgressModel
    {
        public EnumResultStatus Status { get; set; }
        public long TimeSpent { get; set; }
        public DateTime? LastVisited { get; set; }
        public int Visit { get; set; }
        public int? DisplayOrder { get; set; }
        public IList<LessonHomeWorkResultModel>? HomeWorks { get; set; }
    }
}
