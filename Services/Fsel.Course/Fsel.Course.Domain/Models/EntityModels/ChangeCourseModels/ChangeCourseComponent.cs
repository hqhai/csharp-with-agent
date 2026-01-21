// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels
{
    using Fsel.Course.Domain.Entities;

    public abstract class ChangeCourseComponent
    {
        public string Name { get; set; }

        public abstract ChangeCourseDirective? ChangeCourse(ChangeCourseRequest request);

        public abstract ChangeSubjectDirective? ChangeSubject(ChangeProgramRequest request);

        public abstract ChangeSubjectDirective? SelectProjectSubject(ChangeProgramRequest request);

        public abstract bool IsCurrentLearning();

        public abstract FromInfo? GetCurrentInfo();

        public abstract IEnumerable<T> GetComponentsByType<T>() where T : ChangeCourseComponent;

        public abstract bool Contain(Guid targetLevelId);
    }

    public class ChangeCourseRequest
    {
        public Guid LevelId { get; set; }
        public EnumChangeCourseRequest RequestType { get; set; }
    }

    public class ChangeProgramRequest
    {
        public Guid ProgramId { get; set; }
    }

    public class ChangeCourseDirective
    {
        public EnumChangeCourseAction Action { get; set; } = EnumChangeCourseAction.None;
        public Guid ProgramOwnPt { get; set; }
        public Guid ToProgramId { get; set; }
        public Guid ToLevelId { get; set; }
        public Guid? CourseResultId { get; set; }
        public Guid? PtResultId { get; set; }
        public FromInfo? FromInfo { get; set; }
    }

    public class ChangeProgramDirective
    {
        public EnumChangeProgramAction? Action { get; set; }
        public Guid? ProgramOwnPt { get; set; }
        public Guid? RelatedPtResultId { get; set; }
        public Guid ToProgramId { get; set; }
    }

    public class ChangeSubjectDirective
    {
        public EnumChangeSubjectAction Action { get; set; }
        public Guid? CourseResultId { get; set; }
        public Guid? OwnSubjectId { get; set; }
        public DateTime? CreatedOrUpdatedDate { get; set; }
    }

    public enum EnumChangeProgramAction
    {
        None,
        ChangeToProgramExistedPt,
        ChangeDirectlyBecauseByPass,
        ChangeAndStartPt
    }

    public enum EnumChangeCourseRequest
    {
        ChangeCourse,
        ResetCourse,
        ResetCourseAndPt
    }

    public enum EnumChangeSubjectAction
    {
        None,
        ChangeAndStartPt,
        ChangeToRecentCourse
    }
}
