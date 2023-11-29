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
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
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
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly ITrainingService _trainingService;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public GetMockTestRankingQueryHandler(IMapper mapper, IUserService userService, DateTimeConverter dateTimeConverter, ITrainingService trainingService, IMockTestResultRepository mockTestResultRepository)
        {
            _mapper = mapper;
            _userService = userService;
            _dateTimeConverter = dateTimeConverter;
            _trainingService = trainingService;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<MethodResult<IList<TestResultRankingModel>>> Handle(GetMockTestRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TestResultRankingModel>> methodResult = new MethodResult<IList<TestResultRankingModel>>();

            List<TestResultRankingModel> testResultRankings = new List<TestResultRankingModel>();

            var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.MockTestResultId);

            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }

            var currentClass = await _trainingService.GetClassByStudentId(mockTestResult.StudentId);
            var classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();

            var mockTestResults = await _mockTestResultRepository.Queryable
                            .Include(x => x.MockTest)
                            .ThenInclude(x => x!.MockTestSections)
                            .ThenInclude(x => x.SectionGroup)
                            .ThenInclude(x => x.SectionGroupResults)
                            .Where(x => x.MockTestId == mockTestResult.MockTestId && classStudentIds!.Contains(x.StudentId))
                            .ToListAsync(cancellationToken);

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(classStudentIds);
            var students = studentResults?.Content?.Result;

            if (students != null)
            {
                foreach (var item in students)
                {
                    var mockTestResultStudent = mockTestResults.FirstOrDefault(x => x.StudentId == item.Id);
                    var mockTestResultDto = _mapper.Map<TestResultRankingModel>(mockTestResultStudent);
                    if (mockTestResultDto != null)
                    {
                        mockTestResultDto.IsCurrentStudent = item.Id == mockTestResult.StudentId;
                        mockTestResultDto.WorkingTime = mockTestResultStudent?.SectionGroupResults.Select(x => _dateTimeConverter.GetWorkingTime(x.CreatedDate, x.UpdatedDate ?? DateTime.UtcNow, x.SectionGroup!.ExecutionTime)).Sum();
                    }
                    else
                    {
                        mockTestResultDto = new TestResultRankingModel();
                    }
                    mockTestResultDto.FullName = item.Human?.FullName;
                    mockTestResultDto.AvatarPath = item.Human?.AvatarPath;
                    testResultRankings.Add(mockTestResultDto);
                }
            }

            methodResult.Result = testResultRankings.OrderByDescending(x => x.Percent).ThenBy(x => x.FullName).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
