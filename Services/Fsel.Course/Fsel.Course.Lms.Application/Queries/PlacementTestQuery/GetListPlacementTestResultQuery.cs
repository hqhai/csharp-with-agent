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
    using Fsel.Shared.Helpers;
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
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id;
            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId)
                                                                            .OrderBy(x => x.CreatedDate)
                                                                            .ToListAsync(cancellationToken);
            if (!placementTestResults.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = null;
                return methodResult;
            }
            int age = DateTimeHelper.GetYearOld(student?.User?.Birthday);
            var placementTestResultModels = _mapper.Map<IList<PlacementTestResultModel>>(placementTestResults);

            var placementTestResultInitial = placementTestResults.FirstOrDefault();

            foreach (var item in placementTestResultModels)
            {
                var (ptNext, isLock) = item.Level.GetLevelInScore(item.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age));
                item.CourseLevel = ptNext;
                item.IsLock = isLock;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = placementTestResultModels;
            return methodResult;
        }
    }
}
