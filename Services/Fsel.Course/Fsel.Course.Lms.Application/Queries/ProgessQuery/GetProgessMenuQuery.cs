// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgessQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetProgessMenuQuery : IRequest<MethodResult<object>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetProgessMenuQueryHandler : IRequestHandler<GetProgessMenuQuery, MethodResult<object>>
    {
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;

        public GetProgessMenuQueryHandler(AuthContext authContext
            , IMapper mapper
            , IClassForumRepository classForumRepository
            , IUnitRepository unitRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _mapper = mapper;
            _classForumRepository = classForumRepository;
            _unitRepository = unitRepository;
            _userService = userService;
        }

        public async Task<MethodResult<object>> Handle(GetProgessMenuQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;

            var units = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                .ThenInclude(x => x.Lesson)
                .ToListAsync(cancellationToken);
            var lessonIds = units.SelectMany(x => x.UnitLessons).Select(x => x.Lesson).Select(x => x!.Id).ToList();
            var classForumResultScores = new List<ClassForumResultScoreModel>();
            var classForums = await _classForumRepository.Queryable
                                            .Include(x => x.ClassForumResults)
                                            .ThenInclude(x => x.ClassForumScores)
                                            .Where(x => lessonIds.Contains(x.LessonId))
                                            .ToListAsync(cancellationToken);
            var classForumReports = classForums.Select(x => new ClassForumReportModel
            {
                Id = x.Id,
                Title = x.Title,
                CourseSkill = x.CourseSkill,
                ClassForumResultScore = x.ClassForumResults.Where(x => x.StudentId == studentId).Select(x => new ClassForumResultScoreModel
                {
                    Id = x.Id,
                    CorrectCount = x.ClassForumScores.Sum(x => x.Score),
                    TotalCorrect = 30,
                    Status = x.Status,
                    ClassForumScores = _mapper.Map<IList<ClassForumScoreModel>>(x.ClassForumScores)
                }).FirstOrDefault(),
            }).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = classForumReports;
            return methodResult;
        }
    }
}
