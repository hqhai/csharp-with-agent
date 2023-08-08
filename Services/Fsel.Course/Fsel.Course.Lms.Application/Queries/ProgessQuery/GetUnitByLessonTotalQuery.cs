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

    public class GetUnitByLessonTotalQuery : IRequest<MethodResult<UnitResultModel>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetUnitByLessonTotalQueryHandler : IRequestHandler<GetUnitByLessonTotalQuery, MethodResult<UnitResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetUnitByLessonTotalQueryHandler(AuthContext authContext
            , IUnitResultRepository unitResultRepository
            , IMapper mapper
            , IUserService userService)
        {
            _authContext = authContext;
            _unitResultRepository = unitResultRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<UnitResultModel>> Handle(GetUnitByLessonTotalQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UnitResultModel> methodResult = new MethodResult<UnitResultModel>();
            UnitResultModel unitResultModel = new UnitResultModel();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.UnitId == request.UnitId && x.CourseId == request.CourseId, cancellationToken);
            if (unitResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unitResult));
                return methodResult;
            }
            unitResultModel = _mapper.Map<UnitResultModel>(unitResult);
            if (unitResult.SkillScores != null)
            {
                unitResultModel.CorrectCount = unitResult.SkillScores.Sum(x => x.CorrectCount);
                unitResultModel.CorrectTotal = unitResult.SkillScores.Sum(x => x.TotalCount);
                unitResultModel.CountQuestion = unitResult.SkillScores.Sum(x => x.CountQuestion);
                unitResultModel.TotalQuestion = unitResult.SkillScores.Sum(x => x.TotalQuestion);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = unitResultModel;
            return methodResult;
        }
    }
}
