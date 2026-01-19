// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels
{
    using System.Collections.Generic;
    using Fsel.Course.Domain.Entities;

    public class LevelChangeCourse : ChangeCourseComponent
    {
        public bool CanSelect => CanAccess || LearnedBefore || IsCurrentLearningLevel;
        public bool IsCurrentLearningLevel { get; set; }
        private bool LearnedBefore => CourseResultId != null && CourseResultId != Guid.Empty;
        public Guid? CourseResultId { get; set; }
        public Guid LevelId { get; set; }
        public bool CanAccess { get; set; }
        public int LevelOrder { get; set; }

        public override ChangeCourseDirective? ChangeCourse(ChangeCourseRequest request)
        {
            if (request?.LevelId != LevelId)
            {
                return null;
            }

            if (IsCurrentLearningLevel)
            {
                return new ChangeCourseDirective { Action = EnumChangeCourseAction.OverlapLevel, ToLevelId = LevelId, CourseResultId = CourseResultId };
            }

            if (LearnedBefore)
            {
                return new ChangeCourseDirective { Action = EnumChangeCourseAction.SwitchToExistedCourse, ToLevelId = LevelId, CourseResultId = CourseResultId };
            }

            if (CanSelect)
            {
                return new ChangeCourseDirective { Action = EnumChangeCourseAction.ChangeDirectly, ToLevelId = LevelId };
            }

            return new ChangeCourseDirective { Action = EnumChangeCourseAction.ChangeAndStartPt, ToLevelId = LevelId };
        }

        public override FromInfo? GetCurrentInfo()
        {
            if (IsCurrentLearningLevel)
            {
                return new FromInfo { LevelId = LevelId, CourseResultId = CourseResultId };
            }

            return null;
        }

        public override IEnumerable<T> GetComponentsByType<T>()
        {
            if (this is T t)
            {
                yield return t;
            }
        }

        public override bool Contain(Guid targetLevelId)
        {
            return LevelId == targetLevelId;
        }
    }
}
