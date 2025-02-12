// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestResultQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinalTestRankingQuery : IRequest<MethodResult<IList<TestResultRankingModel>>>
    {
        public Guid FinalTestResultId { get; set; }
    }

    public class GetFinalTestRankingQueryHandler : IRequestHandler<GetFinalTestRankingQuery, MethodResult<IList<TestResultRankingModel>>>
    {
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;

        public GetFinalTestRankingQueryHandler(IFinalTestResultRepository finalTestResultRepository, IMapper mapper, IUserService userService, ITrainingService trainingService)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
            _userService = userService;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<IList<TestResultRankingModel>>> Handle(GetFinalTestRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TestResultRankingModel>> methodResult = new MethodResult<IList<TestResultRankingModel>>();
            List<TestResultRankingModel> testResultRankings = new List<TestResultRankingModel>();

            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.FinalTestResultId);

            if (finalTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
                return methodResult;
            }
            var currentClass = await _trainingService.GetClassByStudentId(finalTestResult.StudentId);
            var classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(classStudentIds);
            var students = studentResults?.Content?.Result;

            List<FinalTestResult> finalTestResults = new List<FinalTestResult>();

            if (classStudentIds != null)
            {
                finalTestResults = await _finalTestResultRepository.Queryable
                                                           .WhereBulkContains(classStudentIds, x => x.StudentId)
                                                           .Where(x => x.FinalTestId == finalTestResult.FinalTestId && x.CourseId == finalTestResult.CourseId && x.Status == EnumResultStatus.Done)
                                                           .ToListAsync(cancellationToken);
            }

            if (students != null)
            {
                foreach (var item in students)
                {
                    var finalTestResultStudent = finalTestResults.FirstOrDefault(x => x.StudentId == item.Id);
                    var finalTestResultDto = _mapper.Map<TestResultRankingModel>(finalTestResultStudent);
                    if (finalTestResultStudent == null)
                    {
                        finalTestResultDto = new TestResultRankingModel();
                    }
                    finalTestResultDto.IsCurrentStudent = item.Id == finalTestResult.StudentId;
                    finalTestResultDto.FullName = item.Human?.FullName;
                    finalTestResultDto.AvatarPath = item.Human?.AvatarPath;
                    testResultRankings.Add(finalTestResultDto);
                }
            }

            methodResult.Result = testResultRankings.OrderByDescending(x => x.Status).ThenByDescending(x => x.Percent).ThenBy(x => x.FullName).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
