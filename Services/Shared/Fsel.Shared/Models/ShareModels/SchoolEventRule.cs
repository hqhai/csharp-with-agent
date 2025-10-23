// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SchoolEventRule
    {
        public bool LuckySpin { get; set; }
        public int? PaymentMonth { get; set; }

        /// <summary>
        /// ngày kết thúc gói tự học của học sinh
        /// </summary>
        public DateTime? PaymentDate { get; set; }

        private IList<EnumSchoolEventRuleAction>? _actions;

        public IList<EnumSchoolEventRuleAction>? Actions
        {
            get { return _actions?.Union(ActionConfigs?.Select(x => x.Action) ?? Enumerable.Empty<EnumSchoolEventRuleAction>()).ToList(); }
            set { _actions = value; }
        }

        public string? FselLogo { get; set; }
        public string? EventLogo { get; set; }

        public string? EventTitle { get; set; }
        public string? EventName { get; set; }

        public string? EventDescription { get; set; }

        public string? PromotionalVideo { get; set; }

        public IList<ProcessStep>? ProcessSteps { get; set; }
        public LearningNotice? LearningNotice { get; set; }
        public BannerPromotional? BannerPromotional { get; set; }
        public IList<string>? HowToParticipate { get; set; }

        public string? InstructionalVideo { get; set; }
        public string? InstructionalTitle { get; set; }

        public string? FormDescription { get; set; }
        public string? AwardPromotional { get; set; }
        public IList<string>? AwardPromotionalImage { get; set; }
        public bool IsByPassPayment { get; set; }
        public IList<ActionConfig>? ActionConfigs { get; set; }
        public EnumByPassPaymentType ByPassPaymentType { get; set; }
        public bool IsByPassEmailComfirm { get; set; }
        public DateTime? RegisterStartDate { get; set; }
        public DateTime? RegisterEndDate { get; set; }
        public DateTime? StartDate => WeekEvents?.FirstOrDefault()?.StartDate;
        public DateTime? EndDate => WeekEvents?.LastOrDefault()?.EndDate;
        public DateTime? AwardStartDate { get; set; }
        public DateTime? AwardEndDate { get; set; }
        public string? LinkLeaderBoard { get; set; }
        public string? LinkLuckyStar { get; set; }
        public string? LeaderBoardGiftImage { get; set; }
        public string? LuckySpinGiftImage { get; set; }
        public IList<WeekEvent>? WeekEvents { get; set; }
        public IList<WeekEvent>? LuckyStarRules { get; set; }
        public IList<CollectiveAward>? CollectiveAwards { get; set; }
        public FormConfig? FormConfig { get; set; }
        public Guid? LocationId { get; set; }
        public string? BackGroundImage { get; set; }
        public IList<string>? TabNames { get; set; }
        public ButtonNavSettings? ButtonNavSettings { get; set; }
        public bool? AutoGenAge { get; set; }
        public string? NoticeDescription { get; set; }
        public bool? IsDisplayGift { get; set; }
        public bool? IsParentEvent { get; set; }
    }

    public enum EnumSchoolEventRuleAction
    {
        StopAtLevelSelection,
        ShowStoreFSEL,
        RegisterAndCreateUser,
        DisableLeaderBoard,
        DisableLevelChangeSelection,
        ImportStudent,
        ExportAccount,
        EventGuideScreen,
        WarningScreen,
        DailyQuiz,
        EventRegistrationSuccess,
        EventGiftDisplay
    }

    public enum EnumByPassPaymentType
    {
        Month,
        Date
    }

    #region FormConfig

    public class FormConfig
    {
        public IList<FormSection>? Sections { get; set; }
        public string? Description { get; set; }
        public string? HotLine { get; set; }
    }

    public class FormSection
    {
        public string? Name { get; set; }
        public bool IsShow { get; set; }
        public IList<IList<FormRow>>? Rows { get; set; }
    }

    public class FormRow
    {
        public string? FieldName { get; set; }
        public string? PlaceHolder { get; set; }
        public string? Property { get; set; }
        public bool IsVisible { get; set; }
        public bool Required { get; set; }
        public string? Type { get; set; }
        public string? RequiredMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }

    #endregion FormConfig

    public class ProcessStep
    {
        public string? Content { get; set; }
        public string? Description { get; set; }

        public int Step { get; set; }
    }

    public class ActionConfig
    {
        public EnumSchoolEventRuleAction Action { get; set; }
        public DateTime? EndDate { get; set; }
        public StopAtLevelSelectionConfig? StopAtLevelSelectionConfig { get; set; }
        public IList<StopAtLevelSelectionConfig>? Contents { get; set; }
        public EnumSenderTemplate? MailRegister { get; set; }
        public string? SubjectMailRegister { get; set; }
        public long? StartTime { get; set; }
        public long? EndTime { get; set; }
    }

    public class StopAtLevelSelectionConfig
    {
        public string? Image { get; set; }
        public string? ImageMobile { get; set; }
        public string? Title { get; set; }
        public IList<string>? Content { get; set; }
        public IList<string>? Footer { get; set; }
        public int? Index { get; set; }
    }

    #region Prize

    public class WeekEvent
    {
        public string? Title { get; set; }
        public IList<WeekRule>? Rules { get; set; }
        public int PrizeCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeekNumber { get; set; }
        public EnumLeaderBoardType LeaderBoardType { get; set; }
        public string? FormOfAward { get; set; }
        public string? Rule { get; set; }
        public IList<CollectiveAward>? CollectiveAwards { get; set; }
    }

    public class WeekRule
    {
        public string? PrizeName { get; set; }
        public string? PrizeQuantity { get; set; }
        public string? Detail { get; set; }
        public string? RewardImagine { get; set; }
        public int Rank { get; set; }
    }

    public class CollectiveAward
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    public class LearningNotice
    {
        public string? Title { get; set; }
        public IList<string>? Images { get; set; }
    }

    public class BannerPromotional
    {
        public IList<string>? BannerWeb { get; set; }
        public IList<string>? BannerMobile { get; set; }
    }

    public class ButtonNavSettings
    {
        public string? TextDisplay { get; set; }
        public string? Link { get; set; }
    }

    #endregion Prize
}
