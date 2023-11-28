// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestResultQuery
{
    using System;
    using System.Collections.Generic;
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

    public class GetFinalTestRankingQuery : IRequest<MethodResult<IList<TestResultRankingModel>>>
    {
        public Guid FinalTestResultId { get; set; }
    }

    public class GetFinalTestRankingQueryHandler : IRequestHandler<GetFinalTestRankingQuery, MethodResult<IList<TestResultRankingModel>>>
    {
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ITrainingService _trainingService;

        public GetFinalTestRankingQueryHandler(IFinalTestResultRepository finalTestResultRepository, IMapper mapper, IUserService userService, AuthContext authContext, ITrainingService trainingService)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<IList<TestResultRankingModel>>> Handle(GetFinalTestRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TestResultRankingModel>> methodResult = new MethodResult<IList<TestResultRankingModel>>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;
            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.FinalTestResultId);

            var currentClass = await _trainingService.GetClassByStudentId(finalTestResult!.StudentId);
            var classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();

            var finalTestResults = await _finalTestResultRepository.Queryable
                            .Include(x => x.FinalTest)
                            .ThenInclude(x => x.FinalTestSections)
                            .ThenInclude(x => x.SectionGroup)
                            .Where(x => x.FinalTestId == finalTestResult.FinalTestId && classStudentIds!.Contains(x.StudentId))
                            .ToListAsync(cancellationToken);

            var finalTestResultsDtos = _mapper.Map<IList<TestResultRankingModel>>(finalTestResults);

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(finalTestResultsDtos.Select(x => x.StudentId).ToList());
            var students = studentResults?.Content?.Result?.OrderBy(x => x.Human?.FullName);

            foreach (var item in finalTestResultsDtos)
            {
                item.IsCurrentStudent = item.StudentId == student?.Id;
                item.TimeSpend = DateTimeHelper.GetWorkingTime(item.CreatedDate, item.UpdatedDate ?? DateTime.UtcNow, finalTestResults.Select(x => x.FinalTest!.FinalTestSections.Select(x => x.SectionGroup!.ExecutionTime).FirstOrDefault()).FirstOrDefault());
                item.FullName = students?.FirstOrDefault(x => x.Id == item.StudentId)?.Human?.FullName;
                item.AvatarPath = students?.FirstOrDefault(x => x.Id == item.StudentId)?.Human?.AvatarPath;
            }
            methodResult.Result = finalTestResultsDtos;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
