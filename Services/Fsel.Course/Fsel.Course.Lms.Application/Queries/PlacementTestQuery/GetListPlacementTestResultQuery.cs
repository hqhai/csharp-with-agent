// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListPlacementTestResultQuery : IRequest<MethodResult<IList<PlacementTestResultModel>>>
    {
    }

    public class GetListPlacementTestResultQueryHandler : IRequestHandler<GetListPlacementTestResultQuery, MethodResult<IList<PlacementTestResultModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public GetListPlacementTestResultQueryHandler(AuthContext authContext
            , IUserService userService
            , IMapper mapper
            , IPlacementTestResultRepository placementTestResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<IList<PlacementTestResultModel>>> Handle(GetListPlacementTestResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<PlacementTestResultModel>> methodResult = new MethodResult<IList<PlacementTestResultModel>>();
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId)
                                                                            .OrderByDescending(x => x.CreatedDate)
                                                                            .ToListAsync(cancellationToken);
            if (!placementTestResults.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = null;
                return methodResult;
            }
            var placementTestResultModels = _mapper.Map<IList<PlacementTestResultModel>>(placementTestResults);
            foreach (var item in placementTestResultModels)
            {
                if (item.SkillScores != null && item.SkillScores.Count > 0)
                {
                    item.CountQuestion = item.SkillScores.Sum(x => x.CountQuestion);
                    item.TotalQuestion = item.SkillScores.Sum(x => x.TotalQuestion);
                }
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = placementTestResultModels;
            return methodResult;
        }
    }
}
