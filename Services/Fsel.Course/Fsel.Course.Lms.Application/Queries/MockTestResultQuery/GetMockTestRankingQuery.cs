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
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestRankingQuery : IRequest<MethodResult<IList<MockTestResultRankingModel>>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestRankingQueryHandler : IRequestHandler<GetMockTestRankingQuery, MethodResult<IList<MockTestResultRankingModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ITrainingService _trainingService;
        private readonly IMockTestResultRepository _mockTestResultRepository;

        public GetMockTestRankingQueryHandler(IMapper mapper, IUserService userService, AuthContext authContext, ITrainingService trainingService, IMockTestResultRepository mockTestResultRepository)
        {
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
            _trainingService = trainingService;
            _mockTestResultRepository = mockTestResultRepository;
        }

        public async Task<MethodResult<IList<MockTestResultRankingModel>>> Handle(GetMockTestRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<MockTestResultRankingModel>> methodResult = new MethodResult<IList<MockTestResultRankingModel>>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.MockTestResultId);

            var currentClass = await _trainingService.GetClassByStudentId(student!.Id);
            var classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();

            var mockTestResults = await _mockTestResultRepository.Queryable
                            .Include(x => x.MockTest)
                            .ThenInclude(x => x!.MockTestSections)
                            .ThenInclude(x => x.SectionGroup)
                            .Where(x => x.MockTestId == mockTestResult!.MockTestId && classStudentIds!.Contains(x.StudentId))
                            .ToListAsync(cancellationToken);

            var mockTestResultDtos = _mapper.Map<IList<MockTestResultRankingModel>>(mockTestResults);

            foreach (var item in mockTestResultDtos)
            {
                item.IsCurrentStudent = item.StudentId == student!.Id;
                /*item.TimeSpend = DateTimeHelper.GetWorkingTime(item.CreatedDate, item.UpdatedDate ?? DateTime.UtcNow, finalTestResult!.MockTest!.MockTestSections.Select(x => x.SectionGroup!.ExecutionTime).FirstOrDefault());*/
                item.TimeSpend = DateTimeHelper.GetWorkingTime(item.CreatedDate, item.UpdatedDate ?? DateTime.UtcNow, mockTestResults.Where(x => x.Id == item.Id).Select(x => x.MockTest!.MockTestSections.Select(x => x.SectionGroup!.ExecutionTime).FirstOrDefault()).FirstOrDefault());
            }
            methodResult.Result = mockTestResultDtos;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
