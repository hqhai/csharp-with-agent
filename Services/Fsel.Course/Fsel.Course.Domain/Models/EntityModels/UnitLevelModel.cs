// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class UnitLevelModel
    {
        public IList<CourseLevelUnitModel>? CourseLevelUnits { get; set; }
    }
    public class CourseLevelUnitModel
    {
        public string? CourseLevel { get; set; }
        public IList<UnitModel>? Units { get; set; }
    }
}
