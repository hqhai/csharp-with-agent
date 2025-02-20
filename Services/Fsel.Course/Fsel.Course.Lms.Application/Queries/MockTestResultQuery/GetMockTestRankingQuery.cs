// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestRankingQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<TestResultRankingModel>>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestRankingQueryHandler : IRequestHandler<GetMockTestRankingQuery, MethodResult<PagingItemsModel<TestResultRankingModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public GetMockTestRankingQueryHandler(IMapper mapper,
            IUserService userService,
            IMockTestResultRepository mockTestResultRepository)
        {
            _mapper = mapper;
            _userService = userService;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<MethodResult<PagingItemsModel<TestResultRankingModel>>> Handle(GetMockTestRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<TestResultRankingModel>> methodResult = new MethodResult<PagingItemsModel<TestResultRankingModel>>();

            List<TestResultRankingModel> testResultRankings = new List<TestResultRankingModel>();

            var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.MockTestResultId);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }

            var query = _mockTestResultRepository.Queryable
                                .Where(x => x.CourseId == mockTestResult.CourseId && x.MockTestId == mockTestResult.MockTestId)
                                .Where(x => !mockTestResult.UnitId.HasValue || x.UnitId == mockTestResult.UnitId)
                                .Where(x => x.Status == EnumResultStatus.Done);

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Percent)
                                   .ApplySort(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(lists.Select(x => x.StudentId).ToList());
            var students = studentResults?.Content?.Result;

            foreach (var mockTestResultStudent in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == mockTestResultStudent.StudentId);
                var mockTestResultDto = _mapper.Map<TestResultRankingModel>(mockTestResultStudent);
                mockTestResultDto.IsCurrentStudent = student?.Id == mockTestResult.StudentId;
                mockTestResultDto.FullName = student?.Human?.FullName;
                mockTestResultDto.AvatarPath = student?.Human?.AvatarPath;
                testResultRankings.Add(mockTestResultDto);
            }
            testResultRankings = testResultRankings.OrderByDescending(x => x.Status).ThenByDescending(x => x.Percent).ThenBy(x => x.FullName).ToList();
            methodResult.Result = new PagingItemsModel<TestResultRankingModel>(testResultRankings, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}