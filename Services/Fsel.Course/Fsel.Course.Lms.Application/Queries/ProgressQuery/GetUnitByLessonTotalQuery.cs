// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByLessonTotalQuery : IRequest<MethodResult<UnitResultModel>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetUnitByLessonTotalQueryHandler : IRequestHandler<GetUnitByLessonTotalQuery, MethodResult<UnitResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetUnitByLessonTotalQueryHandler(AuthContext authContext
            , IUnitResultRepository unitResultRepository
            , ICourseResultRepository courseResultRepository
            , IMapper mapper
            , IUserService userService)
        {
            _authContext = authContext;
            _unitResultRepository = unitResultRepository;
            _mapper = mapper;
            _userService = userService;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<UnitResultModel>> Handle(GetUnitByLessonTotalQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UnitResultModel> methodResult = new MethodResult<UnitResultModel>();
            UnitResultModel unitResultModel = new UnitResultModel();
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
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.CourseId == request.CourseId && x.WorkingStatus == Shared.Enums.EnumWorkingStatus.Active, cancellationToken);
            if (courseResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseResultId == courseResult.Id && x.UnitId == request.UnitId, cancellationToken);
            if (unitResult == null)
            {
                return methodResult;
            }
            unitResultModel = _mapper.Map<UnitResultModel>(unitResult);
            if (unitResult.SkillScores != null)
            {
                unitResultModel.CorrectCount = (int)unitResult.SkillScores.Sum(x => x.CorrectCount);
                unitResultModel.CorrectTotal = (int)unitResult.SkillScores.Sum(x => x.TotalCount);
                unitResultModel.CountQuestion = unitResult.SkillScores.Sum(x => x.CountQuestion);
                unitResultModel.TotalQuestion = unitResult.SkillScores.Sum(x => x.TotalQuestion);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = unitResultModel;
            return methodResult;
        }
    }
}
