// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.ClassForumResultCmd
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.StorageServices;
    using Fsel.Course.Lms.Application.Services.StorageServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Polly;

    public class UpdateClassForumDetailResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid ClassForumDetailResultId { get; set; }

        public string? WordContent { get; set; }

        public IList<string>? FilePaths { get; set; }
    }

    public class UpdateClassForumDetailResultCommandHandler : IRequestHandler<UpdateClassForumDetailResultCommand, MethodResult<bool>>
    {
        private readonly IClassForumDetailResultRepository _classForumDetailResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IMediator _mediator;
        private readonly IStorageService _storageService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private const int Max_Time_Retry = 4;
        private const int IntervalRetryTime = 30;
        private const string NameSchema = "criteria_schema";
        private const int MaxTagetScore = 1;
        private const int MaxScoreClassForum = 2;

        public UpdateClassForumDetailResultCommandHandler(IClassForumDetailResultRepository classForumDetailResultRepository,
                                                          IClassForumRepository classForumRepository,
                                                          IMediator mediator,
                                                          IStorageService storageService,
                                                          IClassForumResultRepository classForumResultRepository)
        {
            _classForumDetailResultRepository = classForumDetailResultRepository;
            _classForumRepository = classForumRepository;
            _mediator = mediator;
            _storageService = storageService;
            _classForumResultRepository = classForumResultRepository;
        }

        private class UserAiModel
        {
            public IList<ClassForumAIModel>? ClassForumAIs { get; set; }
            public bool ConditionRetry { get; set; }
        }

        public async Task<MethodResult<bool>> Handle(UpdateClassForumDetailResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classForumDetailResult = await _classForumDetailResultRepository.Queryable
                                                                                .Include(x => x.ClassForumDetailResultHistories)
                                                                                .Include(x => x.ClassForumResult)
                                                                                .Include(x => x.ClassForumResultFiles)
                                                                                .FirstOrDefaultAsync(x => x.Id == request.ClassForumDetailResultId, cancellationToken);
            if (classForumDetailResult == null || classForumDetailResult.ClassForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumDetailResult));
                return methodResult;
            }

            if (classForumDetailResult.ClassForumDetailResultHistories.Count >= 2)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.MaximumEditTwoClassForumDetail), nameof(classForumDetailResult.ClassForumDetailResultHistories));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(classForumDetailResult.GradingAlFeedback))
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotPendingForGradingOrGraded), nameof(classForumDetailResult.GradingAlFeedback));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(classForumDetailResult.ClassForumResult.GradingAlFeedback))
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultStatusNotPendingForGrading), nameof(classForumDetailResult.ClassForumResult.GradingAlFeedback));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable.FirstOrDefaultAsync(x => x.Id == classForumDetailResult.ClassForumResult.ClassForumId, cancellationToken);
            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }

            //  lưu lại bản ghi cũ của class forum detail result
            var filePaths = classForumDetailResult.ClassForumResultFiles.Where(x => x.FilePath != null).Select(x => x.FilePath!).ToList();
            SaveClassForumDetailResultHistory(classForumDetailResult.Content ?? string.Empty, classForumDetailResult.WordContent ?? string.Empty, filePaths, classForumDetailResult);

            if (string.IsNullOrEmpty(request.WordContent) && request.FilePaths != null && request.FilePaths.Any())
            {
                var speechToText = await RetrySpeechToText(request.FilePaths);
                if (string.IsNullOrEmpty(speechToText.Result))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.WordContentNull), nameof(speechToText));
                    return methodResult;
                }

                var retryChatGpt = await RetryChatGpt(speechToText.Result, classForum, cancellationToken);

                classForumDetailResult.WordContent = speechToText.Result;
                classForumDetailResult.GradingAlFeedback = retryChatGpt.Result;
            }
            else if (!string.IsNullOrEmpty(request.WordContent))
            {
                var retryChatGpt = await RetryChatGpt(request.WordContent, classForum, cancellationToken);

                classForumDetailResult.WordContent = request.WordContent;
                classForumDetailResult.GradingAlFeedback = retryChatGpt.Result;
            }

            if (request.FilePaths != null && request.FilePaths.Any())
            {
                var classForumResultFile = classForumDetailResult.ClassForumResultFiles.FirstOrDefault();
                if (classForumResultFile != null)
                {
                    classForumResultFile.FilePath = request.FilePaths.FirstOrDefault();
                }
            }

            await _classForumDetailResultRepository.ExecuteTransactionAsync(async () =>
            {
                if (classForum.CourseSkill == EnumCourseSkill.Writing)
                {
                    classForumDetailResult.Content = classForumDetailResult.WordContent;
                }

                _classForumDetailResultRepository.Update(classForumDetailResult);
                await _classForumDetailResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            var checkForbidden = await _mediator.Send(new CheckForbiddenClassForumCommand { ClassForumDetailResultId = request.ClassForumDetailResultId }, cancellationToken);
            if (!checkForbidden.IsOK)
            {
                methodResult.AddErrorBadRequest(checkForbidden.ErrorMessages);
                return methodResult;
            }

            await UpdateClassForumResultAsync(classForumDetailResult.ClassForumResult, classForumDetailResult);

            return methodResult;
        }

        private async Task<MethodResult<string>> RetryChatGpt(string wordContent, ClassForum classForum, CancellationToken cancellationToken)
        {
            MethodResult<string> methodResult = new MethodResult<string>();
            int countRetry = 0;

            var retryAI = Policy.HandleResult<UserAiModel>(result => result.ClassForumAIs == null || result.ClassForumAIs.Count == 0 || !result.ConditionRetry)
                                    .WaitAndRetryAsync(Max_Time_Retry, retryAttempt => TimeSpan.FromMinutes(IntervalRetryTime), async (result, timeSpan, retryCount, context) =>
                                    {
                                        countRetry += 1;
                                    });

            var retryResult = await retryAI.ExecuteAsync(async () =>
            {
                if (countRetry == Max_Time_Retry)
                {
                    return new UserAiModel
                    {
                        ClassForumAIs = null,
                        ConditionRetry = false
                    };
                }

                var userAiConfig = classForum.UserAlConfig?.Replace("{0}", wordContent, StringComparison.CurrentCulture);

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.SuccessCriteriaSchema);
                var successCriteriaSchema = ConvertHelper.DeserializeFromFilePath<object>(path);

                var aIResponse = await _mediator.Send(new AiCmd.V1i1.SubmitAICommand
                {
                    SettingModel = classForum.SettingModel,
                    SettingTemperature = classForum.SettingTemperature,
                    SettingFrequecy = classForum.SettingFrequecy,
                    SettingWordMaxLength = classForum.SettingWordMaxLength,
                    SettingPresence = classForum.SettingPresence,
                    SettingTopP = classForum.SettingTopP,
                    SystemRoleAlConfig = classForum.SystemRoleAlConfig,
                    UserAIConfig = userAiConfig,
                    Text = successCriteriaSchema,
                    NameSchema = NameSchema
                }, cancellationToken).ConfigureAwait(false);

                aIResponse = Shared.Helpers.StringHelper.RemoveMarkdownFromJson(aIResponse ?? string.Empty);
                try
                {
                    var doc = JsonDocument.Parse(aIResponse);
                    var items = doc.RootElement.GetProperty("parameters");

                    var classForumAIs = GetClassForumAIs(ConvertHelper.Deserialize<List<ClassForumAIModel>?>(items));
                    bool conditionRetry = classForumAIs?.All(x => x != null) ?? default;

                    return new UserAiModel
                    {
                        ClassForumAIs = classForumAIs,
                        ConditionRetry = conditionRetry
                    };

                }
                catch (Exception)
                {
                    return new UserAiModel
                    {
                        ClassForumAIs = null,
                        ConditionRetry = false
                    };
                }
            });

            methodResult.Result = retryResult.ClassForumAIs != null ? ConvertHelper.Serialize(retryResult.ClassForumAIs) : default;
            return methodResult;
        }

        private async Task<MethodResult<string>> RetrySpeechToText(IList<string>? filePaths)
        {
            ArgumentNullException.ThrowIfNull(filePaths);
            MethodResult<string> methodResult = new MethodResult<string>();

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(filePaths.FirstOrDefault());
            if (!response.IsSuccessStatusCode)
            {
                return methodResult;
            }
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "audio.wav";
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "audio/wav";
            var bytes = await response.Content.ReadAsByteArrayAsync();

            var speechToText = await _storageService.ConvertSpeechToText(new ConvertSpeechToTextModel { FileName = fileName, ContentType = contentType, FileData = bytes });

            methodResult.Result = speechToText.Content?.Result;
            return methodResult;
        }

        private static void SaveClassForumDetailResultHistory(string content, string wordContent, IList<string>? filePaths, ClassForumDetailResult classForumDetailResult)
        {
            var newClassForumDetailResultHitory = new ClassForumDetailResultHistory
            {
                Content = content,
                WordContent = wordContent
            };

            if (filePaths != null && filePaths.Any())
            {
                var filePathHistories = filePaths.Select(x => new ClassForumResultFile
                {
                    FilePath = x,

                }).ToList() ?? new List<ClassForumResultFile>();

                foreach (var item in filePathHistories)
                {
                    newClassForumDetailResultHitory.ClassForumResultFiles.Add(item);
                }
            }

            classForumDetailResult.ClassForumDetailResultHistories.Add(newClassForumDetailResultHitory);

        }

        private static IList<ClassForumAIModel>? GetClassForumAIs(List<ClassForumAIModel>? classForumAIs)
        {
            if (classForumAIs == null || !classForumAIs.Any())
            {
                return classForumAIs;
            }

            foreach (var item in classForumAIs)
            {
                item.SuccessCriteriaItemFix = ConvertDataToStrings(item.SuccessCriteriaItemFix);
                item.SuccessCriteriaItemEvidence = ConvertDataToStrings(item.SuccessCriteriaItemEvidence);
            }
            return classForumAIs;
        }

        private static IList<string> ConvertDataToStrings(object? data)
        {
            var listStr = data.Deserialize<IList<string>>();
            if (listStr != null)
            {
                return listStr.ToList();
            }
            return new List<string> { data?.ToString() ?? string.Empty };
        }

        public async Task UpdateClassForumResultAsync(ClassForumResult classForumResult, ClassForumDetailResult classForumDetailResult)
        {
            ArgumentNullException.ThrowIfNull(classForumDetailResult);
            ArgumentNullException.ThrowIfNull(classForumResult);

            var classForumAIs = ConvertHelper.Deserialize<List<ClassForumAIModel>>(classForumDetailResult.GradingAlFeedback);

            bool isForbidden = classForumDetailResult.IsForbiddenWork || classForumDetailResult.IsForbiddenImage;

            classForumResult.Status = !isForbidden ? EnumClassForumResultStatus.Graded : EnumClassForumResultStatus.Denied;
            classForumResult.WordContent = classForumDetailResult.WordContent;
            classForumResult.WordCount = classForumDetailResult.WordCount;
            classForumResult.Content = classForumDetailResult.Content;
            classForumResult.SubmissionCount = classForumDetailResult.SubmissionCount;
            classForumResult.GradingAlFeedback = classForumDetailResult.GradingAlFeedback;

            classForumResult.CorrectCount = GetTargetCount(classForumDetailResult, classForumResult);
            classForumResult.CorrectTotal = MaxTagetScore;

            if (classForumAIs != null && classForumAIs.Any())
            {
                classForumResult.CorrectCount += classForumAIs.Sum(x => x.Score);
                classForumResult.CorrectTotal += classForumAIs.Count * MaxScoreClassForum;
            }

            if (classForumResult.SkillScores != null && classForumResult.SkillScores.Any())
            {
                classForumResult.SkillScores.Single().CorrectCount = classForumResult.CorrectCount;
                classForumResult.SkillScores.Single().TotalCount = classForumResult.CorrectTotal;
            }
            else
            {
                classForumResult.SkillScores = new List<SkillScores>
                {
                    new SkillScores
                    {
                        CorrectCount = classForumResult.CorrectCount,
                        TotalCount = classForumResult.CorrectTotal,
                        CountQuestion = 1,
                        TotalQuestion = 1,
                        Skill = classForumResult.ClassForum?.CourseSkill ?? default
                    }
                };
            }

            await _classForumResultRepository.BulkUpdateList(new List<ClassForumResult> { classForumResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.LessonResultId, c.ClassForumId };
            });

            await _classForumResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
        }

        private static int GetTargetCount(ClassForumDetailResult classForumDetailResult, ClassForumResult classForumResult)
        {
            int targetScore = default;
            var classForum = classForumResult.ClassForum;
            if (classForum?.CourseSkill == EnumCourseSkill.Writing && classForum?.TaggetWordLimit <= classForumDetailResult.WordCount)
            {
                ++targetScore;
            }
            if (classForum?.CourseSkill == EnumCourseSkill.Speaking && classForum?.TaggetTimeLimit <= classForumDetailResult.TimeCount)
            {
                ++targetScore;
            }
            return targetScore;
        }
    }
}
