// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2.Unit
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
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
        private readonly IMapper _mapper;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUserService _userService;
        private readonly IUnitModuleRepository _unitModuleRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public GetUnitByClassForumQueryHandler(AuthContext authContext
            , IClassForumRepository classForumRepository
            , IMapper mapper
            , ILessonResultRepository lessonResultRepository
            , IUserService userService
            , IUnitModuleRepository unitModuleRepository
            , ILessonRepository lessonRepository
            , ILessonModuleRepository lessonModuleRepository
            , IClassForumResultRepository classForumResultRepository)
        {
            _authContext = authContext;
            _classForumRepository = classForumRepository;
            _mapper = mapper;
            _lessonResultRepository = lessonResultRepository;
            _userService = userService;
            _unitModuleRepository = unitModuleRepository;
            _lessonRepository = lessonRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<IList<ClassForumReportModel>>> Handle(GetUnitByClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassForumReportModel>> methodResult = new MethodResult<IList<ClassForumReportModel>>();
            var method = await GetStudentAsync();
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            var studentId = method.Result!.Id;

            methodResult.Result = await GetClassForumReportsAsync(request, studentId);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<StudentModel>> GetStudentAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
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
            methodResult.Result = student;
            return methodResult;
        }

        private async Task<Dictionary<(Guid UnitId, Guid LessonOriginalId), List<(ClassForum, string, Guid?)>>>
        GetClassForumsAsync(GetUnitByClassForumQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var rows = await (
                from um in _unitModuleRepository.ReadQueryable.AsNoTracking()
                join l in _lessonRepository.ReadQueryable.AsNoTracking()
                    on um.OriginalId equals l.OriginalId
                join lm in _lessonModuleRepository.ReadQueryable.AsNoTracking()
                    on l.Id equals lm.LessonId
                join cf in _classForumRepository.ReadQueryable.AsNoTracking()
                    on lm.OriginalId equals cf.OriginalId
                where um.UnitId == request.UnitId
                      && um.UnitConfigType == EnumUnitConfigType.Lesson
                      && l.VersionStatus == EnumVersionStatus.LastVersion
                      && lm.LessonConfigType == EnumLessonConfigType.ClassForum
                      && cf.VersionStatus == EnumVersionStatus.LastVersion
                select new
                {
                    um.UnitId,
                    LessonOriginalId = l.OriginalId,
                    DisplayOrderLesson = um.DisplayOrder,
                    DisplayOrderClassForum = lm.DisplayOrder,
                    ClassForumId = cf.Id,
                    ClassForum = cf,
                    LessonName = l.Name
                }
            ).ToListAsync();

            if (rows.Count == 0)
            {
                return new Dictionary<(Guid UnitId, Guid LessonOriginalId), List<(ClassForum, string, Guid?)>>();
            }
            // Sort ổn định
            var ordered = rows
                .OrderBy(x => x.DisplayOrderLesson)
                .ThenBy(x => x.DisplayOrderClassForum)
                .ThenBy(x => x.ClassForumId) // tie-breaker
                .ToList();

            var dict = ordered
                .GroupBy(x => (x.UnitId, x.LessonOriginalId))
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        // Dedup theo ClassForumId, vẫn giữ đúng thứ tự ordered
                        var seen = new HashSet<Guid>();
                        var list = new List<(ClassForum, string, Guid?)>();

                        foreach (var item in g)
                        {
                            if (!seen.Add(item.ClassForumId))
                            {
                                continue;
                            }
                            list.Add((item.ClassForum, item.LessonName ?? string.Empty, null));
                        }

                        return list;
                    });

            return dict;
        }

        private async Task<Dictionary<(Guid UnitId, Guid LessonOriginalId), IList<(ClassForum, string, Guid?)>>>
            GetClassForumsAsync(GetUnitByClassForumQuery request, Guid studentId)
        {
            var rows = await (from baseQ in _lessonResultRepository.ReadQueryable
                              join l in _lessonRepository.ReadQueryable on baseQ.LessonId equals l.Id
                              join clr in _classForumResultRepository.ReadQueryable on baseQ.Id equals clr.LessonResultId
                              join cl in _classForumRepository.ReadQueryable on clr.ClassForumId equals cl.Id
                              where baseQ.UnitId == studentId && baseQ.UnitId == request.UnitId && baseQ.CourseId == request.CourseId
                              select new
                              {
                                  UnitId = baseQ.UnitId,
                                  LessonOriginalId = l.OriginalId,
                                  ClassForum = cl,
                                  LessonName = l.Name,
                                  ClassForumResultId = clr.Id,
                              }).ToListAsync();

            // Group theo (UnitId, LessonOriginalId) -> list forums
            var dict = rows.GroupBy(x => (x.UnitId, x.LessonOriginalId))

                .ToDictionary(
                               g => g.Key,
                               g => (IList<(ClassForum, string, Guid?)>)g
                                   .Select(x => (x.ClassForum, x.LessonName, x.ClassForumResultId))
                                   .ToList()
                           );

            return dict;
        }

        private async Task<IList<ClassForumResult>> GetClassForumResultsAsync(GetUnitByClassForumQuery request, Guid studentId)
        {
            var classForumResults = await (from baseQ in _lessonResultRepository.ReadQueryable
                                           join l in _lessonRepository.ReadQueryable on baseQ.LessonId equals l.Id
                                           join clr in _classForumResultRepository.ReadQueryable.Include(x => x.ClassForumDetailResults) on baseQ.Id equals clr.LessonResultId
                                           where baseQ.UnitId == studentId && baseQ.UnitId == request.UnitId && baseQ.CourseId == request.CourseId
                                           select clr).ToListAsync();

            return classForumResults;
        }

        private async Task<IList<ClassForumReportModel>> GetClassForumReportsAsync(
            GetUnitByClassForumQuery request,
            Guid studentId)
        {
            ArgumentNullException.ThrowIfNull(request);

            var classForumsConfigDict = await GetClassForumsAsync(request);
            var classForumsStudentDict = await GetClassForumsAsync(request, studentId);

            var mergedDict = new Dictionary<(Guid UnitId, Guid LessonOriginalId), IList<(ClassForum, string, Guid?)>>();
            foreach (var kv in classForumsConfigDict)
            {
                mergedDict[kv.Key] = kv.Value;
            }
            foreach (var kv in classForumsStudentDict)
            {
                if (!mergedDict.TryGetValue(kv.Key, out var existing))
                {
                    mergedDict[kv.Key] = kv.Value;
                    continue;
                }

                mergedDict[kv.Key] = existing
                    .Concat(kv.Value)
                    .GroupBy(x => x.Item1.Id)
                    .Select(g => g.First())
                    .ToList();
            }

            if (mergedDict.Count == 0)
            {
                return new List<ClassForumReportModel>();
            }

            var classForumResults = await GetClassForumResultsAsync(request, studentId);
            return mergedDict.SelectMany(x => x.Value).Select(x =>
            {
                var classForumResult = classForumResults.FirstOrDefault(y => y.Id == x.Item3);
                var classForumReport = _mapper.Map<ClassForumReportModel>(x.Item1);
                classForumReport.Name = x.Item2;
                classForumReport.LessonResultId = classForumResult?.LessonResultId;
                if (classForumResult != null)
                {
                    classForumReport.ClassForumResultScore = new ClassForumResultScoreModel
                    {
                        Id = classForumResult.Id,
                        CorrectCount = classForumResult.CorrectCount,
                        TotalCorrect = classForumResult.CorrectTotal,
                        Percent = classForumResult.Percent,
                        Status = classForumResult.Status,
                        ProcessDate = classForumResult.ClassForumDetailResults.Where(x => x.ProcessDate.HasValue).OrderBy(x => x.CreatedDate).FirstOrDefault()?.ProcessDate
                    };
                }

                return classForumReport;
            }).ToList();
        }
    }
}
