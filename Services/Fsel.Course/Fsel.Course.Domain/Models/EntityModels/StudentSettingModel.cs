// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public class StudentSettingModel
    {
        public Guid StudentId { get; set; }
        public bool IsPlacementTest { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? CompetitionEventId { get; set; }
        public EnumCourseLevel? Level { get; set; }
        public EnumCourseLevel? BaseCourseLevel { get; set; }
        public Guid? PTLevel { get; set; }
        public EnumCourseLevel? StartPTLevel { get; set; }
        public IList<EnumSchoolEventRuleAction>? Actions { get; set; }
        public IList<ActionConfig>? ActionConfigs { get; set; }
        public int ModuleNumber { get; set; }
        public bool IsLockPT { get; set; }
        public EnumTrialRegistrationStatus? Status { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public double NumberOfToken { get; set; }
        public bool IsSurveyEvent { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsActivedAccount { get; set; } = true;
        public bool TurnOnTouchpoint { get; set; }
        public EnumUserStatus? UserStatus { get; set; }
        public Guid? RootSubjectId { get; set; }
        public string? RootSubjectName { get; set; }
        public bool VstepSetting { get; set; }
        public StudentBeginnerGuideModel? BeginnerGuide { get; set; }
        public CourseModel? Course { get; set; }
    }
}
