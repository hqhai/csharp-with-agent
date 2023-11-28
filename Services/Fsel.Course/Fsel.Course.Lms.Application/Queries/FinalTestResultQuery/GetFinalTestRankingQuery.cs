// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestResultQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
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

            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.FinalTestResultId);

            var currentClass = await _trainingService.GetClassByStudentId(finalTestResult!.StudentId);
            var classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();

            var finalTestResults = await _finalTestResultRepository.Queryable
                            .Include(x => x.FinalTest)
                            .ThenInclude(x => x!.FinalTestSections)
                            .ThenInclude(x => x.SectionGroup)
                            .ThenInclude(x => x!.SectionGroupResults)
                            .Where(x => x.FinalTestId == finalTestResult.FinalTestId && classStudentIds!.Contains(x.StudentId))
                            .ToListAsync(cancellationToken);

            var finalTestResultDtos = _mapper.Map<IList<TestResultRankingModel>>(finalTestResults);

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(finalTestResultDtos.Select(x => x.StudentId).ToList());
            var students = studentResults?.Content?.Result?.OrderBy(x => x.Human?.FullName);

            foreach (var item in finalTestResultDtos)
            {
                item.IsCurrentStudent = item.StudentId == finalTestResult?.StudentId;
                item.WorkingTime = finalTestResults.FirstOrDefault(x => x.Id == item.Id)?.SectionGroupResults.Select(x => DateTimeHelper.GetWorkingTime(x.CreatedDate, x.UpdatedDate ?? DateTime.UtcNow, x.SectionGroup!.ExecutionTime)).Sum();
                item.FullName = students?.FirstOrDefault(x => x.Id == item.StudentId)?.Human?.FullName;
                item.AvatarPath = students?.FirstOrDefault(x => x.Id == item.StudentId)?.Human?.AvatarPath;
            }
            methodResult.Result = finalTestResultDtos;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
