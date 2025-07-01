// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForums.V1i1
{
    using Fsel.Course.Domain.Enums;

    public class CreateClassForumCommandModel
    {
        public string? PromptName { get; set; }

        public EnumGradingStyle GradingStyle { get; set; }

        public long TaggetWordLimit { get; set; }

        public double TaggetTimeLimit { get; set; }

        public string? MediaPost { get; set; }

        public bool IsAlFeedBack { get; set; }

        public string? SystemRoleAlConfig { get; set; }

        public string? UserAlConfig { get; set; }

        public string? SettingModel { get; set; }

        public EnumClassForumLayout Layout { get; set; }

        public Guid SkillId { get; set; }

        public Guid ProgramId { get; set; }

        public double SettingTemperature { get; set; }

        public double SettingWordMaxLength { get; set; }

        public double SettingTopP { get; set; }

        public double SettingFrequecy { get; set; }

        public double SettingPresence { get; set; }

        public IList<string>? FilePaths { get; set; }
    }
}
