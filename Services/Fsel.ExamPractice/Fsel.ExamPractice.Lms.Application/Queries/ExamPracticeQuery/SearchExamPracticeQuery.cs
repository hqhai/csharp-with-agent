// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery
{
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SearchExamPracticeQuery : BaseQueryModel, IRequest<MethodResult<IList<ExamPracticeGroupTypeModel>>>
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

    public class SearchExamPracticeQueryHandler : IRequestHandler<SearchExamPracticeQuery, MethodResult<IList<ExamPracticeGroupTypeModel>>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private const int TotalRetry = 20;

        public SearchExamPracticeQueryHandler(IExamPracticeRepository examPracticeRepository,
            IUserService userService,
            AuthContext authContext)
        {
            _examPracticeRepository = examPracticeRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<ExamPracticeGroupTypeModel>>> Handle(SearchExamPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<ExamPracticeGroupTypeModel>>();

            var student = await GetStudentAsync(methodResult);
            if (student == null)
            {
                return methodResult;
            }
            var query = _examPracticeRepository.Queryable.Where(x => x.Type == request.Type)
                    .Where(x => x.Status == EnumExamPracticeStatus.Active || x.ExamPracticeResults.Any(y => y.StudentId == student.Id));
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.Name != null && x.Name.Contains(request.Keyword));
            }
            if (request.SubType.HasValue)
            {
                query = query.Where(x => x.SubType == request.SubType);
            }
            if (request.Type == EnumExamPracticeType.IELTS && request.CourseSkill.HasValue)
            {
                query = query.Where(x => x.ExamPracticeSections.Any(y => y.CourseSkill == request.CourseSkill.Value));
            }
            var data = await query.Select(x => new
            {
                CourseSkills = x.ExamPracticeSections.Select(x => x.CourseSkill).ToList(),
                ExamPractice = x,
                ExamPracticeSections = x.ExamPracticeSections.ToList(),
                ParticipantCount = x.ExamPracticeResults.Count,
                ExamPracticeRetry = x.ExamPracticeRetrys.FirstOrDefault(y => y.StudentId == student.Id),
                ExamPracticeResult = x.ExamPracticeResults.FirstOrDefault(y => y.StudentId == student.Id && y.ExamPracticeId == x.Id && y.WorkingStatus == EnumWorkingStatus.Active),
            }).ToListAsync(cancellationToken);

            var inactiveStatuses = new List<EnumExamPracticeStatus> { EnumExamPracticeStatus.Cloned, EnumExamPracticeStatus.Inactive };
            if (request.ProgressStatuses != null && request.ProgressStatuses.Any())
            {
                data = data.Where(x => MatchProgressStatus(x, request.ProgressStatuses)).ToList();
            }

            var list = data.Where(x => !inactiveStatuses.Contains(x.ExamPractice.Status))
            .GroupBy(x => new
            {
                x.ExamPractice.SubType,
                CourseSkillsKey = string.Join(",", x.ExamPracticeSections
                                                     .Where(s => s.CourseSkill.HasValue)
                                                     .Select(s => s.CourseSkill!.Value)
                                                     .OrderBy(s => s)) // Quan trọng: sắp xếp để key nhất quán
            })
            .Select(g => new ExamPracticeGroupTypeModel
            {
                SubType = g.Key.SubType,
                CourseSkills = g.SelectMany(x => x.ExamPracticeSections)
                                .Where(x => x.CourseSkill.HasValue)
                                .Select(x => x.CourseSkill!.Value)
                                .Distinct()
                                .ToList(),
                ExamPracticeGroupModels = g.Select(x => BuildExamPracticeGroupModel(x, x.ExamPractice, x.ExamPracticeSections, x.ExamPracticeResult))
                                           .ApplySort(request).ToList()
            }).ToList();

            var dataClone = data.Where(x => inactiveStatuses.Contains(x.ExamPractice.Status)).GroupBy(_ => string.Empty)
            .Select(g => new ExamPracticeGroupTypeModel
            {
                ExamPracticeGroupModels = g.Select(x => BuildExamPracticeGroupModel(x, x.ExamPractice, x.ExamPracticeSections, x.ExamPracticeResult))
                                           .ApplySort(request).ToList()
            }).ToList();
            list.AddRange(dataClone);

            methodResult.Result = list;
            return methodResult;
        }

        private async Task<StudentModel?> GetStudentAsync(MethodResult<IList<ExamPracticeGroupTypeModel>> result)
        {
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                result.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return null;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return null;
            }
            return student;
        }

        private static bool MatchProgressStatus(dynamic x, IList<EnumPracticeProgressStatus> selectedStatuses)
        {
            var result = x.ExamPracticeResult;
            return
                (selectedStatuses.Contains(EnumPracticeProgressStatus.NotStarted) && result == null) ||
                (selectedStatuses.Contains(EnumPracticeProgressStatus.InProgress) && result != null && result.Status != EnumResultStatus.Done) ||
                (selectedStatuses.Contains(EnumPracticeProgressStatus.Completed) && result != null && result.Status == EnumResultStatus.Done);
        }

        private static ExamPracticeGroupModel BuildExamPracticeGroupModel(dynamic x, ExamPractice examPractice, IList<ExamPracticeSection>? examPracticeSections, ExamPracticeResult? examPracticeResult)
        {
            return new ExamPracticeGroupModel
            {
                Id = examPractice.Id,
                CreatedDate = examPractice.CreatedDate,
                UpdatedDate = examPractice.UpdatedDate,
                Code = examPractice.Code,
                Type = examPractice.Type,
                ActivatedAt = examPractice.ActivatedAt,
                ExamPracticeStatus = examPractice.Status,
                ExecutionTime = examPractice.ExecutionTime,
                Name = examPractice.Name,
                IsNew = examPractice.ActivatedAt.HasValue && DateTime.UtcNow <= examPractice.ActivatedAt.Value.AddDays(7),
                ParticipantCount = x.ParticipantCount,
                Status = examPracticeResult?.Status,
                ExamPracticeResultId = examPracticeResult?.Id,
                Config = examPracticeResult?.Config,
                ExamPracticeScore = examPracticeResult?.ExamPracticeScore,
                CorrectCount = examPracticeResult?.CorrectCount ?? default,
                CorrectTotal = examPracticeResult?.CorrectTotal ?? default,
                PracticeMode = examPracticeResult?.PracticeMode,
                TotalSections = examPracticeSections?.Count ?? default,
                TotalQuestions = examPracticeSections?.Select(s => s.Config?.TotalQuestion).Sum() ?? default,
                TotalRetry = x.ExamPracticeRetry?.RetryCount ?? TotalRetry,
                Score = examPracticeResult?.SkillScores?.Any() == true
                                    ? NumberHelper.RoundNumberDouble(examPracticeResult.SkillScores.Average(s => s.Scores))
                                    : default,
            };
        }
    }
}
