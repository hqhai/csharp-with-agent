// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TrainingServices.Models
{
    public class GetClassListByStudentIdsModel
    {
        public IList<Guid>? StudentIds { get; set; }
        public Guid? CourseId { get; set; }
    }
}
