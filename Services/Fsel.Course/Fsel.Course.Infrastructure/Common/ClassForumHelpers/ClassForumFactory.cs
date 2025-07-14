// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.ClassForumHelpers
{
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.ClassForums.V1i1;

    public class ClassForumFactory
    {
        private readonly CreateClassForumCommandModel _createRequest;

        public ClassForumFactory(CreateClassForumCommandModel createRequest)
        {
            _createRequest = createRequest;
        }

        public ClassForum Build(int version = 0, Guid? originalId = null)
        {
            var classForum = new ClassForum
            {
                PromptName = _createRequest.PromptName,
                GradingStyle = _createRequest.GradingStyle,
                TaggetWordLimit = _createRequest.TaggetWordLimit,
                TaggetTimeLimit = _createRequest.TaggetTimeLimit,
                MediaPost = _createRequest.MediaPost,
                IsAlFeedBack = _createRequest.IsAlFeedBack,
                SystemRoleAlConfig = _createRequest.SystemRoleAlConfig,
                UserAlConfig = _createRequest.UserAlConfig,
                SettingModel = _createRequest.SettingModel,
                Layout = _createRequest.Layout,
                SkillId = _createRequest.SkillId,
                ProgramId = _createRequest.ProgramId,
                SettingTemperature = _createRequest.SettingTemperature,
                SettingWordMaxLength = _createRequest.SettingWordMaxLength,
                SettingTopP = _createRequest.SettingTopP,
                SettingFrequecy = _createRequest.SettingFrequecy,
                SettingPresence = _createRequest.SettingPresence,
                ClassForumFiles = _createRequest.FilePaths?.Select(x => new ClassForumFile
                {
                    FilePath = x,
                }).ToList() ?? new List<ClassForumFile>(),
                VersionStatus = EnumVersionStatus.LastVersion,
                Version = version
            };

            classForum.OriginalId = originalId.HasValue ? originalId.Value : classForum.Id;

            return classForum;
        }

        public static ClassForumFactory Create(CreateClassForumCommandModel createRequest)
        {
            return new ClassForumFactory(createRequest);
        }
    }
}
