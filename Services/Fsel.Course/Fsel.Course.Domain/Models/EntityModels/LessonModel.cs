// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class LessonModel : BaseModel
    {
        public string? Name { get; set; }
        public string? InstructionContent { get; set; }
        public string? Thumbnail { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? DisplayOrder { get; set; }

        public int? Number
        {
            get { return DisplayOrder; }
        }

        public EnumCourseLevel CourseLevel { get; set; }
        public Guid? VideoId { get; set; }
        public VideoModel? Video { get; set; }
        public LessonResultModel? LessonResult { get; set; }
        public IList<HomeWorkModel>? HomeWorks { get; set; }
        public IList<Guid>? ExtraPracticeIds { get; set; }
        public IList<LessonInstructionModel>? LessonInstructions { get; set; }
        public ClassForumModel? ClassForum { get; set; }
        public bool IsClassForumLock { get; set; } = true;
        public bool IsHomeWorkLock { get; set; } = true;
    }
}
