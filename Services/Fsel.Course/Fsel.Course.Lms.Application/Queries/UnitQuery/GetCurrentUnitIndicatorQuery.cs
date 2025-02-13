// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCurrentUnitIndicatorQuery : IRequest<MethodResult<IList<SkillScores>>>
    {
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? ObjectId { get; set; }
    }

    public class GetCurrentUnitIndicatorQueryHandler : IRequestHandler<GetCurrentUnitIndicatorQuery, MethodResult<IList<SkillScores>>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;

        public GetCurrentUnitIndicatorQueryHandler(IUserService userService
            , AuthContext authContext
            , IFinalTestResultRepository finalTestResultRepository
            , IMockTestRepository mockTestRepository
            , IFinalTestRepository finalTestRepository
            , IMockTestResultRepository mockTestResultRepository
            , IUnitResultRepository unitResultRepository)
        {
            _userService = userService;
            _authContext = authContext;
            _finalTestResultRepository = finalTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _finalTestRepository = finalTestRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _unitResultRepository = unitResultRepository;
        }

        public async Task<MethodResult<IList<SkillScores>>> Handle(GetCurrentUnitIndicatorQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SkillScores>> methodResult = new MethodResult<IList<SkillScores>>();

            #region Validate

            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            #endregion Validate

            request.ObjectId = request.ObjectId ?? request.UnitId;
            if (await _finalTestRepository.AnyGuidAsync(request.ObjectId ?? default))
            {
                var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.CourseId == request.CourseId && x.FinalTestId == request.ObjectId)
                    .FirstOrDefaultAsync(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done, cancellationToken);
                if (finalTestResult == null)
                {
                    methodResult.Result = new List<SkillScores>();
                    return methodResult;
                }
                methodResult.Result = finalTestResult.SkillScores;
            }
            else if (await _mockTestRepository.AnyGuidAsync(request.ObjectId ?? default))
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.Where(x => x.CourseId == request.CourseId && x.MockTestId == request.ObjectId)
                    .FirstOrDefaultAsync(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done, cancellationToken);
                if (mockTestResult == null)
                {
                    methodResult.Result = new List<SkillScores>();
                    return methodResult;
                }
                methodResult.Result = mockTestResult.SkillScores;
            }
            else
            {
                var unitResult = await GetUnitResult(request, studentId, cancellationToken);
                if (unitResult == null)
                {
                    methodResult.Result = new List<SkillScores>();
                    return methodResult;
                }
                methodResult.Result = unitResult.SkillScores;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<UnitResult?> GetUnitResult(GetCurrentUnitIndicatorQuery request, Guid studentId, CancellationToken cancellationToken)
        {
            var currentUnitIndicator = await _unitResultRepository
                              .Queryable
                              .Where(x => x.CourseId == request.CourseId && x.Status == EnumResultStatus.Done && x.StudentId == studentId
                                && (request.UnitId == null || x.UnitId == request.UnitId))
                              .Select(x => new
                              {
                                  UnitResult = x,
                                  MaxDisplayOrder = x.Course!.CourseUnitMockTests
                                                    .Where(cumt => cumt.UnitId == x.UnitId && cumt.CourseId == x.CourseId)
                                                    .Max(cumt => cumt.DisplayOrder)
                              })
                              .OrderByDescending(x => x.MaxDisplayOrder)
                              .FirstOrDefaultAsync(cancellationToken);
            return currentUnitIndicator?.UnitResult;
        }
    }
}
