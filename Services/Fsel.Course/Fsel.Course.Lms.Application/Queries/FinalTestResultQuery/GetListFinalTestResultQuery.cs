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

    public class GetListFinalTestResultQuery : IRequest<MethodResult<IList<FinalTestResultModel>>>
    {
        public Guid FinalTestResultId { get; set; }
    }

    public class GetListFinalTestResultQueryHandler : IRequestHandler<GetListFinalTestResultQuery, MethodResult<IList<FinalTestResultModel>>>
    {
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ITrainingService _trainingService;

        public GetListFinalTestResultQueryHandler(IFinalTestResultRepository finalTestResultRepository, IMapper mapper, IUserService userService, AuthContext authContext, ITrainingService trainingService)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<IList<FinalTestResultModel>>> Handle(GetListFinalTestResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FinalTestResultModel>> methodResult = new MethodResult<IList<FinalTestResultModel>>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            IList<Guid>? classStudentIds = new List<Guid>();

            var currentClass = await _trainingService.GetClassByStudentId(student!.Id);
            classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();

            var finalTestResults = await _finalTestResultRepository.Queryable
                            .Include(x => x.FinalTest)
                            .Where(x => x.Id == request.FinalTestResultId && classStudentIds!.Contains(x.StudentId))
                            .ToListAsync(cancellationToken);

            var finalTestResultsDtos = _mapper.Map<IList<FinalTestResultModel>>(finalTestResults);
            foreach (var item in finalTestResultsDtos)
            {
                var finalTestResult = finalTestResults.FirstOrDefault(x => x.Id == item.Id);
                item.IsCurrentStudent = item.StudentId == student!.Id;
                item.TimeSpend = DateTimeHelper.GetWorkingTime(item.CreatedDate, item.UpdatedDate ?? DateTime.UtcNow, finalTestResult?.FinalTest?.ExecutionTime ?? default);
            }
            methodResult.Result = finalTestResultsDtos;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
