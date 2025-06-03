// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
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

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
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

            var list = data
            .GroupBy(x => new
            {
                x.ExamPractice.SubType,
                CourseSkillsKey = string.Join(",", x.ExamPracticeSections
                                                     .Where(s => s.CourseSkill.HasValue)
                                                     .Select(s => s.CourseSkill.Value)
                                                     .OrderBy(s => s)) // Quan trọng: sắp xếp để key nhất quán
            })
            .Select(g => new ExamPracticeGroupTypeModel
            {
                SubType = g.Key.SubType,
                CourseSkills = g.SelectMany(x => x.ExamPracticeSections)
                                .Where(x => x.CourseSkill.HasValue)
                                .Select(x => x.CourseSkill.Value)
                                .Distinct()
                                .ToList(),
                ExamPracticeGroupModels = g.Select(x => new ExamPracticeGroupModel
                {
                    Id = x.ExamPractice.Id,
                    CreatedDate = x.ExamPractice.CreatedDate,
                    Code = x.ExamPractice.Code,
                    Type = x.ExamPractice.Type,
                    ExamPracticeStatus = x.ExamPractice.Status,
                    ExecutionTime = x.ExamPractice.ExecutionTime,
                    Name = x.ExamPractice.Name,
                    IsNew = x.ExamPractice.ActivatedAt.HasValue && DateTime.UtcNow <= x.ExamPractice.ActivatedAt.Value.AddDays(7),
                    ParticipantCount = x.ParticipantCount,
                    TotalSections = x.ExamPracticeSections.Count,
                    TotalQuestions = x.ExamPracticeSections.Select(s => s.Config?.TotalQuestion).Sum() ?? 0,
                    Status = x.ExamPracticeResult?.Status,
                    ExamPracticeResultId = x.ExamPracticeResult?.Id,
                    Config = x.ExamPracticeResult?.Config,
                    ExamPracticeScore = x.ExamPracticeResult?.ExamPracticeScore,
                    CorrectCount = x.ExamPracticeResult?.CorrectCount ?? default,
                    CorrectTotal = x.ExamPracticeResult?.CorrectTotal ?? default,
                    PracticeMode = x.ExamPracticeResult?.PracticeMode,
                    Score = x.ExamPracticeResult?.SkillScores != null && x.ExamPracticeResult.SkillScores.Any() ? NumberHelper.RoundNumberDouble(x.ExamPracticeResult.SkillScores.Average(x => x.Scores)) : default,
                    TotalRetry = (x.ExamPracticeRetry?.RetryCount ?? TotalRetry),
                }).OrderByDescending(x => x.CreatedDate).ToList()
            }).ToList();

            methodResult.Result = list;
            return methodResult;
        }
    }
}
