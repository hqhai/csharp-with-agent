// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.LessonHelpers
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1;
    using Fsel.Course.Infrastructure.Common.ClassForumHelpers;
    using Fsel.Course.Infrastructure.Common.DocumentHelpers;
    using Microsoft.EntityFrameworkCore;

    public class LessonConverter
    {
        private readonly Dictionary<EnumLessonConfigType, Func<UpdateLessonModuleModel, bool, bool, CancellationToken, Task<VoidMethodResult>>> _lessonModule;
        private readonly IVideoRepository _videoRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IVersionEntityUpdater<ClassForum> _versionEntityUpdaterClassForum;
        private readonly IVersionEntityUpdater<Document> _versionEntityUpdaterDocument;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;
        private const double MinPercent = 99;
        private const double MaxPercent = 100;
        private const int MinLenghtName = 150;
        private const int MinLenghtFilePath = 3000;
        private static readonly Regex s_regexCode = new Regex("^[a-zA-Z0-9._]+$", RegexOptions.Compiled);
        private static readonly Regex s_regexInstructionContent = new Regex("^[^<>&#*]{1,2000}$", RegexOptions.Compiled);
        private static readonly Regex s_regexInstruction = new Regex("^[^<>&#*]{1,1000}$", RegexOptions.Compiled);

        public LessonConverter(IVideoRepository videoRepository,
                               IHomeWorkRepository homeWorkRepository,
                               ISkillRepository skillRepository,
                               IClassForumRepository classForumRepository,
                               IDocumentRepository documentRepository,
                               IVersionEntityUpdater<ClassForum> versionEntityUpdaterClassForum,
                               IVersionEntityUpdater<Document> versionEntityUpdaterDocument,
                               ILessonRepository lessonRepository,
                               ICategoryRepository categoryRepository,
                               ILevelRepository levelRepository)
        {
            _lessonModule = new Dictionary<EnumLessonConfigType, Func<UpdateLessonModuleModel, bool, bool, CancellationToken, Task<VoidMethodResult>>>()
            {
                { EnumLessonConfigType.Video, LessonModuleVideoHandler },
                { EnumLessonConfigType.ClassForum, LessonModuleClassForumHandler},
                { EnumLessonConfigType.HomeWork, LessonModuleHomeWorkHandler},
                { EnumLessonConfigType.Document, LessonModuleDocumentHandler}
            };
            _videoRepository = videoRepository;
            _homeWorkRepository = homeWorkRepository;
            _skillRepository = skillRepository;
            _classForumRepository = classForumRepository;
            _documentRepository = documentRepository;
            _versionEntityUpdaterClassForum = versionEntityUpdaterClassForum;
            _versionEntityUpdaterDocument = versionEntityUpdaterDocument;
            _lessonRepository = lessonRepository;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
        }

        public async Task<VoidMethodResult> LessonModuleHandler(IList<UpdateLessonModuleModel> lessonModules, bool isUpdate, bool isNewVersion, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonModules);
            var methodResult = new VoidMethodResult();

            // handler lesson module
            foreach (var request in lessonModules)
            {
                var fun = _lessonModule[request.LessonConfigType];
                var handler = await fun(request, isUpdate, isNewVersion, cancellationToken);
                if (!handler.IsOK)
                {
                    methodResult.AddErrorBadRequest(handler.ErrorMessages);
                    return methodResult;
                }
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> ValidateLesson(UpdateLessonCommandModel request, bool isUpdate, Guid? originId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (string.IsNullOrEmpty(request.Name))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.CodeNotNullOrEmpty), request.Name);
                return methodResult;
            }

            if (!s_regexCode.IsMatch(request.Name))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.CodeNotValid), request.Name);
                return methodResult;
            }

            var checkCode = await _lessonRepository.Queryable.AnyAsync(x => (isUpdate ? (x.Id != request.Id && x.OriginalId != originId) : (!originId.HasValue)) && x.Name == request.Name.Trim(), cancellationToken);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.CodeAlreadyExist), request.Name);
                return methodResult;
            }

            var checkProgram = await _categoryRepository.AnyGuidAsync(request.ProgramId);
            if (!checkProgram)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ProgramId));
                return methodResult;
            }

            var checkLevel = await _levelRepository.AnyGuidAsync(request.LevelId);
            if (!checkLevel)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.LevelId));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.InstructionContent) && !s_regexInstructionContent.IsMatch(request.InstructionContent))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.InstructionContentNotValid), request.InstructionContent);
                return methodResult;
            }

            if (request.LessonModules == null || !request.LessonModules.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonModuleNotNull), nameof(request.LessonModules));
                return methodResult;
            }

            if (request.LessonInstructions != null && request.LessonInstructions.Any(request => !string.IsNullOrEmpty(request.Instruction) && !s_regexInstruction.IsMatch(request.Instruction)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.InstructionNotValid));
                return methodResult;
            }

            double sumPercent = request.LessonModules.Sum(x => x.Percent);
            if (sumPercent <= MinPercent || sumPercent > MaxPercent)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.PercentNotValid), nameof(sumPercent), sumPercent);
                return methodResult;
            }

            var openOrders = request.LessonModules.Select(x => x.OpenOrder).Distinct().ToList();
            if (!IsValidNumber(openOrders))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.OpenOrderOutOfSequence), nameof(openOrders), openOrders);
                return methodResult;
            }

            var videoIds = request.LessonModules.Where(x => x.LessonConfigType == EnumLessonConfigType.Video && x.OriginalId.HasValue).Select(x => x.OriginalId!.Value).ToList() ?? new List<Guid>();
            var checkVideo = await _videoRepository.Queryable.WhereBulkContains(videoIds, x => x.OriginalId).CountAsync(cancellationToken: cancellationToken);
            if (videoIds.Any() && checkVideo != videoIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(checkVideo));
                return methodResult;
            }

            var homeWorkIds = request.LessonModules.Where(x => x.LessonConfigType == EnumLessonConfigType.HomeWork && x.OriginalId.HasValue).Select(x => x.OriginalId!.Value).ToList() ?? new List<Guid>();
            var checkHomeWork = await _homeWorkRepository.Queryable.WhereBulkContains(homeWorkIds, x => x.OriginalId).CountAsync(cancellationToken: cancellationToken);
            if (homeWorkIds.Any() && checkHomeWork != homeWorkIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(checkHomeWork));
                return methodResult;
            }

            return methodResult;
        }

        private async Task<VoidMethodResult> LessonModuleVideoHandler(UpdateLessonModuleModel request, bool isUpdate, bool isNewVersion, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<bool>();
            return await Task.FromResult(methodResult);
        }

        private async Task<VoidMethodResult> LessonModuleHomeWorkHandler(UpdateLessonModuleModel request, bool isUpdate, bool isNewVersion, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<bool>();
            return await Task.FromResult(methodResult);
        }

        private async Task<VoidMethodResult> LessonModuleDocumentHandler(UpdateLessonModuleModel request, bool isUpdate, bool isNewVersion, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<Guid>();

            if (request.Document == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.DocumentNotNull), nameof(request.Document));
                return methodResult;
            }

            if (request.Document.Files == null || !request.Document.Files.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.DocumentNotNull), nameof(request.Document.Files));
                return methodResult;
            }

            foreach (var file in request.Document.Files)
            {
                if (string.IsNullOrEmpty(file.Name) || string.IsNullOrEmpty(file.FilePath))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumDocumentErrorCode.NameFileOrFilePathNull), nameof(file.FilePath));
                    return methodResult;
                }

                if (file.Name.Length > MinLenghtName)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumDocumentErrorCode.NameFileMaxLength), nameof(file.Name));
                    return methodResult;
                }

                if (file.FilePath.Length > MinLenghtFilePath)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumDocumentErrorCode.FilePathMaxLength), nameof(file.FilePath));
                    return methodResult;
                }
            }

            if (!isUpdate)
            {
                var document = DocumentFactory.Create(request.Document).Build(version: 0, originalId: Guid.NewGuid());
                if (!document.IsValid())
                {
                    methodResult.AddErrorBadRequest(document.ErrorMessages);
                    return methodResult;
                }

                await _documentRepository.ExecuteTransactionAsync(async () =>
                {
                    _documentRepository.Add(document);
                    await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    request.OriginalId = document.OriginalId;
                    return methodResult;
                });
            }
            else
            {
                var document = await _documentRepository.Queryable
                                                        .Where(x => x.OriginalId == request.OriginalId && x.VersionStatus == EnumVersionStatus.LastVersion)
                                                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (document == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(document));
                    return methodResult;
                }

                var newVersionDocument = DocumentFactory.Create(request.Document).Build(version: 0, originalId: Guid.NewGuid());
                if (!newVersionDocument.IsValid())
                {
                    methodResult.AddErrorBadRequest(document.ErrorMessages);
                    return methodResult;
                }

                await _versionEntityUpdaterDocument.UpdateEntity(document, newVersionDocument,
                async (_, entity) => isNewVersion,
                async (oldEntity, newEntity) =>
                {
                    oldEntity.Files = newEntity.Files;
                    await Task.Yield();
                });

                request.OriginalId = document.OriginalId;
            }

            return methodResult;
        }

        private async Task<VoidMethodResult> LessonModuleClassForumHandler(UpdateLessonModuleModel request, bool isUpdate, bool isNewVersion, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<Guid>();

            if (request.ClassForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.ClassForumNotNull), nameof(request.ClassForum));
                return methodResult;
            }

            if (request.ClassForum.IsAlFeedBack && string.IsNullOrEmpty(request.ClassForum.SystemRoleAlConfig) && string.IsNullOrEmpty(request.ClassForum.UserAlConfig))
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.AiConfigNotNull), nameof(request.ClassForum));
                return methodResult;
            }

            var checkSkill = await _skillRepository.AnyGuidAsync(request.ClassForum.SkillId);
            if (!checkSkill)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ClassForum.SkillId), request.ClassForum.SkillId);
                return methodResult;
            }

            if (!isUpdate)
            {
                var classForum = ClassForumFactory.Create(request.ClassForum).Build(version: 0, originalId: Guid.NewGuid());
                if (!classForum.IsValid())
                {
                    methodResult.AddErrorBadRequest(classForum.ErrorMessages);
                    return methodResult;
                }

                await _classForumRepository.ExecuteTransactionAsync(async () =>
                {
                    _classForumRepository.Add(classForum);
                    await _classForumRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    request.OriginalId = classForum.OriginalId;
                    return methodResult;
                });
            }
            else
            {
                var classForum = await _classForumRepository.Queryable
                                                            .Where(x => x.OriginalId == request.OriginalId && x.VersionStatus == EnumVersionStatus.LastVersion)
                                                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (classForum == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                    return methodResult;
                }

                var newVersionClassForum = ClassForumFactory.Create(request.ClassForum).Build(version: 0, originalId: Guid.NewGuid());
                if (!newVersionClassForum.IsValid())
                {
                    methodResult.AddErrorBadRequest(newVersionClassForum.ErrorMessages);
                    return methodResult;
                }

                await _versionEntityUpdaterClassForum.UpdateEntity(classForum, newVersionClassForum,
                async (_, entity) => isNewVersion,
                async (oldEntity, newEntity) =>
                {
                    oldEntity.PromptName = oldEntity.PromptName;
                    oldEntity.GradingStyle = oldEntity.GradingStyle;
                    oldEntity.TaggetWordLimit = oldEntity.TaggetWordLimit;
                    oldEntity.TaggetTimeLimit = oldEntity.TaggetTimeLimit;
                    oldEntity.MediaPost = oldEntity.MediaPost;
                    oldEntity.IsAlFeedBack = oldEntity.IsAlFeedBack;
                    oldEntity.SystemRoleAlConfig = oldEntity.SystemRoleAlConfig;
                    oldEntity.UserAlConfig = oldEntity.UserAlConfig;
                    oldEntity.SettingModel = oldEntity.SettingModel;
                    oldEntity.Layout = oldEntity.Layout;
                    oldEntity.SkillId = oldEntity.SkillId;
                    oldEntity.ProgramId = oldEntity.ProgramId;
                    oldEntity.SettingTemperature = oldEntity.SettingTemperature;
                    oldEntity.SettingWordMaxLength = oldEntity.SettingWordMaxLength;
                    oldEntity.SettingTopP = oldEntity.SettingTopP;
                    oldEntity.SettingFrequecy = oldEntity.SettingFrequecy;
                    oldEntity.SettingPresence = oldEntity.SettingPresence;
                    oldEntity.ClassForumFiles = oldEntity.ClassForumFiles;
                    await Task.Yield();
                });

                request.OriginalId = classForum.OriginalId;
            }

            return await Task.FromResult(methodResult);
        }

        private static bool IsValidNumber(List<int> numbers)
        {
            if (numbers == null || !numbers.Any())
            {
                return true;
            }

            numbers.Sort();

            for (int i = 1; i < numbers.Count; i++)
            {
                if (numbers[i] != numbers[i - 1] + 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
