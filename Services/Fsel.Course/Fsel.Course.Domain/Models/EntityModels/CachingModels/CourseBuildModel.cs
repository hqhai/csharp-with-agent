// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.CachingModels
{
    using Fsel.Course.Domain.Enums;

    public class CourseBuildModel
    {
        public Guid CourseId { get; set; }
        public IList<CourseModuleBuildModel> CourseModules { get; set; } = new List<CourseModuleBuildModel>();
    }

    public class BaseModuleModel
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
        public int DisplayNumber { get; set; }
        public double Percent { get; set; }
        public int OpenOrder { get; set; }
    }

    public class CourseModuleBuildModel : BaseModuleModel
    {
        public EnumCourseConfigType ConfigType { get; set; }
        public Guid? TestId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid OriginalId { get; set; }
        public IList<UnitModuleBuildModel> UnitModuleBuilds { get; set; } = new List<UnitModuleBuildModel>();
    }

    public class UnitModuleBuildModel : BaseModuleModel
    {
        public EnumUnitConfigType ConfigType { get; set; }
        public Guid? TestId { get; set; }
        public Guid? LessonId { get; set; }
        public Guid OriginalId { get; set; }
        public IList<LessonModuleBuildModel> LessonModuleBuilds { get; set; } = new List<LessonModuleBuildModel>();
    }

    public class LessonModuleBuildModel : BaseModuleModel
    {
        public EnumLessonConfigType ConfigType { get; set; }
        public Guid? ClassForumId { get; set; }
        public Guid? VideoId { get; set; }
        public Guid? HọmeWorkId { get; set; }
        public Guid? DocumentId { get; set; }
        public Guid OriginalId { get; set; }
    }
}
