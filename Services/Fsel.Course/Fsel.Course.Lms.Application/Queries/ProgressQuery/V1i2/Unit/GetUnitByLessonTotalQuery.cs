// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2.Unit
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
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

            var method = await GetStudentAsync();
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            var studentId = method.Result!.Id;

            var unitResult = await _unitResultRepository.ReadQueryable
                                                        .Where(x => x.StudentId == studentId && x.UnitId == request.UnitId)
                                                        .FirstOrDefaultAsync(x => x.CourseId == request.CourseId, cancellationToken);
            if (unitResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unitResult));
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
    }
}
