// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByClassForumQuery : IRequest<MethodResult<IList<ClassForumReportModel>>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetUnitByClassForumQueryHandler : IRequestHandler<GetUnitByClassForumQuery, MethodResult<IList<ClassForumReportModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;

        public GetUnitByClassForumQueryHandler(AuthContext authContext
            , IClassForumRepository classForumRepository
            , IUnitRepository unitRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _classForumRepository = classForumRepository;
            _unitRepository = unitRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<ClassForumReportModel>>> Handle(GetUnitByClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumReportModel>> methodResult = new MethodResult<IList<ClassForumReportModel>>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;

            var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                .ThenInclude(x => x.Lesson)
                .Include(x => x.CourseUnitMockTests)
                .FirstOrDefaultAsync(x => x.Id == request.UnitId && x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId && x.UnitId == request.UnitId), cancellationToken);

            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }

            var lessonIds = unit.UnitLessons.Select(x => x.Lesson).Select(x => x!.Id).ToList();
            var classForumResultScores = new List<ClassForumResultScoreModel>();
            var classForums = await _classForumRepository.Queryable
                                            .Include(x => x.Lesson)
                                            .ThenInclude(x => x.LessonResults.Where(x => x.StudentId == studentId))
                                            .Include(x => x.Lesson)
                                            .ThenInclude(x => x.UnitLessons)
                                            .Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                            .ThenInclude(x => x.ClassForumScores)
                                            .Where(x => lessonIds.Contains(x.LessonId))
                                            .OrderBy(x => x.Lesson!.UnitLessons.Where(n => n.UnitId == unit.Id).Select(n => n.DisplayOrder).FirstOrDefault())
                                            .ToListAsync(cancellationToken);
            if (classForums == null || classForums.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForums));
                return methodResult;
            }
            var classForumReports = classForums.Select(x => new ClassForumReportModel
            {
                Id = x.Id,
                Name = x.Lesson?.Name,
                CourseSkill = x.CourseSkill,
                TotalCorrect = 36,
                LessonId = x.LessonId,
                GradingStyle = x.GradingStyle,
                LessonResultId = x.Lesson?.LessonResults.FirstOrDefault(y => y.LessonId == x.LessonId && y.StudentId == studentId)?.Id,
                ClassForumResultScore = x.ClassForumResults.Select(x => new ClassForumResultScoreModel
                {
                    Id = x.Id,
                    CorrectCount = x.ClassForumScores.Count > 0 ? x.ClassForumScores.Sum(x => x.Score) : default,
                    TotalCorrect = 36,
                    Status = x.Status,
                }).FirstOrDefault(),
            }).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = classForumReports;
            return methodResult;
        }
    }
}
