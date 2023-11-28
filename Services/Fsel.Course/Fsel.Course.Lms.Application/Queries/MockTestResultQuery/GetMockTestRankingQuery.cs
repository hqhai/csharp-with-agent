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
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestRankingQuery : IRequest<MethodResult<IList<TestResultRankingModel>>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestRankingQueryHandler : IRequestHandler<GetMockTestRankingQuery, MethodResult<IList<TestResultRankingModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public GetMockTestRankingQueryHandler(IMapper mapper, IUserService userService, ITrainingService trainingService, IMockTestResultRepository mockTestResultRepository)
        {
            _mapper = mapper;
            _userService = userService;
            _trainingService = trainingService;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<MethodResult<IList<TestResultRankingModel>>> Handle(GetMockTestRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TestResultRankingModel>> methodResult = new MethodResult<IList<TestResultRankingModel>>();

            var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.MockTestResultId);

            var currentClass = await _trainingService.GetClassByStudentId(mockTestResult!.StudentId);
            var classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();

            var mockTestResults = await _mockTestResultRepository.Queryable
                            .Include(x => x.MockTest)
                            .ThenInclude(x => x!.MockTestSections)
                            .ThenInclude(x => x.SectionGroup)
                            .ThenInclude(x => x.SectionGroupResults)
                            .Where(x => x.MockTestId == mockTestResult!.MockTestId && classStudentIds!.Contains(x.StudentId))
                            .ToListAsync(cancellationToken);

            var mockTestResultDtos = _mapper.Map<IList<TestResultRankingModel>>(mockTestResults);

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(mockTestResultDtos.Select(x => x.StudentId).ToList());
            var students = studentResults?.Content?.Result?.OrderBy(x => x.Human?.FullName);

            foreach (var item in mockTestResultDtos)
            {
                item.IsCurrentStudent = item.StudentId == mockTestResult?.Id;
                item.WorkingTime = mockTestResults.FirstOrDefault(x => x.Id == item.Id)?.SectionGroupResults.Select(x => DateTimeHelper.GetWorkingTime(x.CreatedDate, x.UpdatedDate ?? DateTime.UtcNow, x.SectionGroup!.ExecutionTime)).Sum();
                item.FullName = students?.FirstOrDefault(x => x.Id == item.StudentId)?.Human?.FullName;
                item.AvatarPath = students?.FirstOrDefault(x => x.Id == item.StudentId)?.Human?.AvatarPath;
            }
            methodResult.Result = mockTestResultDtos;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
