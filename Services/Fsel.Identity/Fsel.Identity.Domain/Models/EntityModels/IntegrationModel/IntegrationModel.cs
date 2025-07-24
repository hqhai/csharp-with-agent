// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class IntegrationModel
    {
        public Guid UserId { get; set; }

        public string? FullName { get; set; }

        public string? UserName { get; set; }

        public string? StudentEmail { get; set; }

        public string? StudentPhone { get; set; }

        public EnumGender? Gender { get; set; }

        public DateTime? Birthday { get; set; }

        public string? Address { get; set; }

        public string? ParentName { get; set; }

        public string? ParentPhone { get; set; }

        public string? ParentEmail { get; set; }

        public EnumGender? ParentGender { get; set; }

        public Guid? SchoolId { get; set; }

        public string? LongPathSchool { get; set; }

        public string? LongPathLocation { get; set; }

        public DateTime? LastDate { get; set; }

        public EnumCourseLevel? PTLevel { get; set; }

        public string? CourseLevel { get; set; }

        public string? CurrentUnit { get; set; }

        public string? CurrentLesson { get; set; }

        public int? LessonCompleted { get; set; }

        public DateTime? DateEdit { get; set; }

        public DateTime? CreateAccount { get; set; }

        public string? SchoolGrade { get; set; }

        public string? SchoolClass { get; set; }

        public string? HumanCode { get; set; }

        public string? OTPPhoneNumber { get; set; }

        public string? OTPEmail { get; set; }

        public EnumCourseLevel CurrentLevel { get; set; }

        public string? EventCode { get; set; }

        public EnumIntegrationStatus? Status { get; set; }

        public long? AccessTime { get; set; }

        public DateTime? ExpireDate { get; set; }

        public IList<OrderIntegrationModel>? OrderIntegrations { get; set; }

        public IList<CourseIntegrationModel>? CourseIntegrations { get; set; }

        public IList<CourseSuggestModel>? CourseSuggests { get; set; }

        public IList<IntegrationPlacementTestResultModels>? PlacementTestResults { get; set; }
    }


    public class IntegrationPlacementTestResultModels
    {
        public EnumPlacementTestLevel Level { get; set; }

        public int CorrectCount { get; set; }

        public int CorrectTotal { get; set; }

        public IList<SkillScores>? SkillScores { get; set; }
    }

    public class SkillScores
    {
        [JsonRequired]
        public EnumCourseSkill Skill { get; set; }

        [JsonRequired]
        public double Scores { get; set; }

        [JsonRequired]
        public double TotalCount { get; set; }

        [JsonRequired]
        public double CorrectCount { get; set; }

        [JsonRequired]
        public double TotalQuestion { get; set; }

        [JsonRequired]
        public double CountQuestion { get; set; }

        private double _percent;

        [JsonRequired]
        public double Percent
        {
            get
            {
                return TotalCount > 0 ? NumberHelper.GetPercent(CorrectCount, TotalCount) : _percent;
            }
            set { _percent = TotalCount > 0 ? NumberHelper.GetPercent(CorrectCount, TotalCount) : value; }
        }
    }
}
