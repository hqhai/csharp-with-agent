// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
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
        private readonly IMapper _mapper;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUserService _userService;

        public GetUnitByClassForumQueryHandler(AuthContext authContext
            , IClassForumRepository classForumRepository
            , IUnitRepository unitRepository
            , IMapper mapper
            , ILessonResultRepository lessonResultRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _classForumRepository = classForumRepository;
            _unitRepository = unitRepository;
            _mapper = mapper;
            _lessonResultRepository = lessonResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<ClassForumReportModel>>> Handle(GetUnitByClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumReportModel>> methodResult = new MethodResult<IList<ClassForumReportModel>>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;
            methodResult.Result = await GetClassForumReportsAsync(request, studentId);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<Guid>> GetLessonIdsAsync(GetUnitByClassForumQuery request)
        {
            return await _unitRepository.ReadQueryable.Include(x => x.UnitLessons)
                                                  .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId && x.UnitId == request.UnitId))
                                                  .SelectMany(x => x.UnitLessons)
                                                  .OrderBy(x => x.DisplayOrder)
                                                  .Select(x => x.LessonId)
                                                  .ToListAsync();
        }

        private async Task<IList<LessonResult>> GetLessonResultsAsync(GetUnitByClassForumQuery request, Guid studentId)
        {
            return await _lessonResultRepository.ReadQueryable.Where(x => x.StudentId == studentId && x.UnitId == request.UnitId && x.CourseId == request.CourseId).ToListAsync();
        }

        private async Task<IList<ClassForumReportModel>> GetClassForumReportsAsync(GetUnitByClassForumQuery request, Guid studentId)
        {
            var lessonIds = await GetLessonIdsAsync(request);
            if (!lessonIds.Any())
            {
                return new List<ClassForumReportModel>();
            }

            var lessonResults = await GetLessonResultsAsync(request, studentId);
            var lessonResultIds = lessonResults.Select(x => x.Id).ToList();

            var classForums = await _classForumRepository.ReadQueryable.Include(x => x.Lesson)
                                                           .Include(x => x.ClassForumResults.Where(x => lessonResultIds.Contains(x.LessonResultId)))
                                                           .ThenInclude(x => x.ClassForumDetailResults)
                                                           .WhereBulkContains(lessonIds, x => x.LessonId)
                                                           .ToListAsync();

            return classForums.OrderBy(x => lessonIds.IndexOf(x.LessonId)).Select(x =>
            {
                var lessonResult = lessonResults.FirstOrDefault(y => y.LessonId == x.LessonId);
                var classForumReport = _mapper.Map<ClassForumReportModel>(x);
                classForumReport.Name = x.Lesson?.Name;
                classForumReport.LessonResultId = lessonResult?.Id;
                classForumReport.ClassForumResultScore = x.ClassForumResults.Select(x => new ClassForumResultScoreModel
                {
                    Id = x.Id,
                    CorrectCount = x.CorrectCount,
                    TotalCorrect = x.CorrectTotal,
                    Percent = x.Percent,
                    Status = x.Status,
                    ProcessDate = x.ClassForumDetailResults.Where(x => x.ProcessDate.HasValue).OrderBy(x => x.CreatedDate).FirstOrDefault()?.ProcessDate
                }).FirstOrDefault();
                return classForumReport;
            }).ToList();
        }
    }
}
