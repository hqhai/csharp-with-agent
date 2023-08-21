// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPlacementTestResultQuery : IRequest<MethodResult<PlacementTestResultModel>>
    {
    }

    public class GetPlacementTestResultQueryHandler : IRequestHandler<GetPlacementTestResultQuery, MethodResult<PlacementTestResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public GetPlacementTestResultQueryHandler(AuthContext authContext
            , IUserService userService
            , IMapper mapper
            , IPlacementTestResultRepository placementTestResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<PlacementTestResultModel>> Handle(GetPlacementTestResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestResultModel> methodResult = new MethodResult<PlacementTestResultModel>();
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var placementTestResult = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId)
                                                                            .OrderByDescending(x => x.CreatedDate)
                                                                            .FirstOrDefaultAsync(cancellationToken);
            if (placementTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTestResult));
                return methodResult;
            }
            var placementTestResultModel = _mapper.Map<PlacementTestResultModel>(placementTestResult);
            if (placementTestResult.SkillScores != null)
            {
                placementTestResultModel.CountQuestion = placementTestResult.SkillScores.Sum(x => x.CountQuestion);
                placementTestResultModel.TotalQuestion = placementTestResult.SkillScores.Sum(x => x.TotalQuestion);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = placementTestResultModel;
            return methodResult;
        }
    }
}
