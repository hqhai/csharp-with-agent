// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery.V1i1
{
    using System.Text.Json.Serialization;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Entities.BandScoresConfigs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SearchExamPracticeQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ExamPracticeGroupModel>>>
    {
        public EnumExamPracticeType Type { get; set; }
        public EnumExamPracticeSubType? SubType { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public string? ProgressStatusStr { get; set; }

        [JsonIgnore]
        public IList<EnumPracticeProgressStatus>? ProgressStatuses
        {
            get
            {
                return ProgressStatusStr.ToList<EnumPracticeProgressStatus>();
            }
        }
    }

    public class SearchExamPracticeQueryHandler : IRequestHandler<SearchExamPracticeQuery, MethodResult<PagingItemsModel<ExamPracticeGroupModel>>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IExamPracticeAnswerRepository _examPracticeAnswerRepository;
        private const int TotalRetry = 20;

        public SearchExamPracticeQueryHandler(IExamPracticeRepository examPracticeRepository,
            IUserService userService,
            AuthContext authContext,
            IMapper mapper,
            IExamPracticeAnswerRepository examPracticeAnswerRepository)
        {
            _examPracticeRepository = examPracticeRepository;
            _userService = userService;
            _authContext = authContext;
            _mapper = mapper;
            _examPracticeAnswerRepository = examPracticeAnswerRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ExamPracticeGroupModel>>> Handle(SearchExamPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ExamPracticeGroupModel>>();
            var student = await GetStudentAsync(methodResult);
            if (student == null)
            {
                return methodResult;
            }
            request.SortBy.Add(new GenericSortModel
            {
                Property = nameof(ExamPractice.Name),
                IsDesc = false
            });

            var query = _examPracticeRepository.Queryable.Where(x => x.Type == request.Type)
                                               .Where(x => x.Status == EnumExamPracticeStatus.Active || x.ExamPracticeResults.Any(y => y.StudentId == student.Id));
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.Name != null && x.Name.Contains(request.Keyword));
            }
            if (request.ProgressStatuses != null && request.ProgressStatuses.Any())
            {
                var statuses = request.ProgressStatuses.ToList();
                query = query.Where(x =>
                    (statuses.Contains(EnumPracticeProgressStatus.NotStarted) && !x.ExamPracticeResults.Any(y => y.StudentId == student.Id && y.WorkingStatus == EnumWorkingStatus.Active)) ||
                    (statuses.Contains(EnumPracticeProgressStatus.InProgress) && x.ExamPracticeResults.Any(y => y.StudentId == student.Id && y.WorkingStatus == EnumWorkingStatus.Active && y.Status != EnumResultStatus.Done)) ||
                    (statuses.Contains(EnumPracticeProgressStatus.Completed) && x.ExamPracticeResults.Any(y => y.StudentId == student.Id && y.WorkingStatus == EnumWorkingStatus.Active && y.Status == EnumResultStatus.Done))
                );
            }

            if (request.SubType.HasValue)
            {
                query = query.Where(x => x.SubType == request.SubType);
            }

            if (request.CourseSkill.HasValue)
            {
                query = query.Where(x => x.ExamPracticeSections.Any(y => y.CourseSkill.HasValue && y.CourseSkill == request.CourseSkill.Value));
            }
            var queryTest = query.Select(x => new
            {
                ExamPractice = x,
                ExamPracticeSections = x.ExamPracticeSections.Where(y => !y.ParentExamPracticeSectionId.HasValue).ToList(),
                ParticipantCount = x.ExamPracticeResults.Count,
                ExamPracticeRetry = x.ExamPracticeRetrys.FirstOrDefault(y => y.StudentId == student.Id),
                ExamPracticeResult = x.ExamPracticeResults.FirstOrDefault(y => y.StudentId == student.Id && y.ExamPracticeId == x.Id && y.WorkingStatus == EnumWorkingStatus.Active),
            });
            int totalItem = await queryTest.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await queryTest
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var examPracticeResultIds = lists.Where(x => x.ExamPractice.SubType == EnumExamPracticeSubType.SkillMockTest || x.ExamPractice.SubType == EnumExamPracticeSubType.SingleVstepSkill
                                                || x.ExamPractice.Type == EnumExamPracticeType.ExamPractice)
                                       .Where(x => x.ExamPracticeResult != null)
                                       .Select(x => x.ExamPracticeResult!.Id)
                                       .ToList();

            var examPracticeAnswers = await _examPracticeAnswerRepository.Queryable.AsNoTracking().Where(x => x.QuestionId.HasValue)
                                                                         .WhereBulkContains(examPracticeResultIds, x => x.ExamPracticeResultId)
                                                                         .ToListAsync(cancellationToken);
            var examPracticeAnswerGroups = examPracticeAnswers.GroupBy(x => x.ExamPracticeResultId).ToDictionary(g => g.Key, g => g.ToList());
            var data = lists.Select(x =>
            {
                examPracticeAnswerGroups.TryGetValue(x.ExamPracticeResult?.Id ?? Guid.Empty, out var answers);
                return BuildExamPracticeGroupModel(x, x.ExamPractice, x.ExamPracticeSections, x.ExamPracticeResult, answers?.Count ?? default);
            }).ToList();
            methodResult.Result = new PagingItemsModel<ExamPracticeGroupModel>(data, request, totalItem);
            return methodResult;
        }

        private async Task<StudentModel?> GetStudentAsync(MethodResult<PagingItemsModel<ExamPracticeGroupModel>> result)
        {
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                result.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return default;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return default;
            }
            return student;
        }

        private ExamPracticeGroupModel BuildExamPracticeGroupModel(dynamic x, ExamPractice examPractice, IList<ExamPracticeSection>? examPracticeSections, ExamPracticeResult? examPracticeResult, double countAnswer)
        {
            var examPracticeModel = _mapper.Map<ExamPracticeGroupModel>(examPractice);
            var courseSkills = examPracticeSections?.Where(x => x.CourseSkill.HasValue).Select(x => x.CourseSkill!.Value).Distinct().ToList() ?? new List<EnumCourseSkill>();
            examPracticeModel.CourseSkills = courseSkills;
            if (examPracticeSections != null)
            {
                if (examPractice.Type is (EnumExamPracticeType.Vstep or EnumExamPracticeType.IELTS))
                {
                    examPracticeModel.ExecutionTime = examPracticeSections.Sum(x => x.Config?.ExecutionTime ?? default);
                }
                examPracticeModel.TotalSection = examPracticeSections.Count;
                examPracticeModel.TotalQuestion = examPracticeSections.Select(s => s.Config?.TotalQuestion ?? default).Sum();
            }
            if (examPracticeResult != null)
            {
                SetDataResult(examPracticeModel, examPracticeResult);
                var skills = examPracticeResult.SkillScores?.Select(s => s.Skill);
                examPracticeModel.RemainingSkills = skills != null && skills.Any()
                                                    ? courseSkills.Except(skills).ToList()
                                                    : courseSkills;

                #region ProgressPercent

                var skillScore = examPracticeResult.SkillScores?.FirstOrDefault();
                if (examPractice.Type is (EnumExamPracticeType.Vstep or EnumExamPracticeType.IELTS))
                {
                    if (examPractice.SubType is EnumExamPracticeSubType.FullMockTest or EnumExamPracticeSubType.FullVstepSkill)
                    {
                        examPracticeModel.ProgressPercent = skills != null && skills.Any()
                           ? NumberHelper.GetPercent(skills.Count(), courseSkills.Count)
                           : default;
                    }
                    if (examPractice.SubType is EnumExamPracticeSubType.SingleVstepSkill or EnumExamPracticeSubType.SkillMockTest)
                    {
                        if (skillScore != null && courseSkills.Any(x => x == EnumCourseSkill.Reading || x == EnumCourseSkill.Listening))
                        {
                            examPracticeModel.ProgressPercent = NumberHelper.GetPercent(countAnswer, examPracticeModel.TotalQuestion);
                        }
                        else
                        {
                            var maxPercent = 100;
                            examPracticeModel.ProgressPercent = examPracticeResult.Status == EnumResultStatus.Done ? maxPercent : default;
                        }
                    }
                }
                else
                {
                    examPracticeModel.ProgressPercent = NumberHelper.GetPercent(countAnswer, examPracticeModel.TotalQuestion);
                }

                #endregion ProgressPercent
            }

            examPracticeModel.ParticipantCount = x.ParticipantCount;
            examPracticeModel.TotalRetry = x.ExamPracticeRetry?.RetryCount ?? TotalRetry;
            return examPracticeModel;
        }

        private static void SetDataResult(ExamPracticeGroupModel examPracticeModel, ExamPracticeResult examPracticeResult)
        {
            examPracticeModel.Status = examPracticeResult.Status;
            examPracticeModel.ExamPracticeResultId = examPracticeResult.Id;
            examPracticeModel.Config = examPracticeResult.Config;
            examPracticeModel.ExamPracticeScore = examPracticeResult.ExamPracticeScore;
            examPracticeModel.CorrectCount = examPracticeResult.CorrectCount;
            examPracticeModel.CorrectTotal = examPracticeResult.CorrectTotal;
            examPracticeModel.PracticeMode = examPracticeResult.PracticeMode;
            if (examPracticeResult.SkillScores?.Any() == true)
            {
                examPracticeModel.Score = NumberHelper.RoundNumberDouble(examPracticeResult.SkillScores.Average(s => s.Scores));
            }
            if (examPracticeModel.Type == EnumExamPracticeType.Vstep && examPracticeModel.Score.HasValue && examPracticeResult.Status == EnumResultStatus.Done)
            {
                var pathConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.ExamBandScores);
                var bandScores = ConvertHelper.DeserializeFromFilePath<IList<BandScores>>(pathConfig);
                examPracticeModel.ScoreLevel = bandScores?.Where(x => examPracticeModel.Score >= x.MinInclusive)
                                                          .FirstOrDefault(x => examPracticeModel.Score <= x.MaxInclusive)?.Level;
            }
        }
    }
}
