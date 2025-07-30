// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class UnitModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<LessonModel>? Lessons { get; set; }
        public UnitResultModel? UnitResult { get; set; }
        public MockTestModel? SkillMockTest { get; set; }
        public double Percent { get; set; }
        public int Version { get; set; }
        public Guid OriginalId { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? LevelId { get; set; }
        public IList<UnitModuleDTO>? UnitModules { get; set; }
    }

    public class UnitModuleDTO
    {
        public EnumUnitConfigType UnitConfigType { get; set; }

        public int DisplayOrder { get; set; }

        public int DisplayNumber { get; set; }

        public double Percent { get; set; }

        public int OpenOrder { get; set; }

        public Guid OriginalId { get; set; }

    }
}
