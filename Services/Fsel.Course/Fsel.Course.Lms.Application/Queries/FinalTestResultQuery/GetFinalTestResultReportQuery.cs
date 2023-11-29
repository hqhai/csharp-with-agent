// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinalTestResultReportQuery : IRequest<MethodResult<TestResultRankingModel>>
    {
        public Guid FinalTestResultId { get; set; }
    }

    public class GetFinalTestResultReportQueryHandler : IRequestHandler<GetFinalTestResultReportQuery, MethodResult<TestResultRankingModel>>
    {
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetFinalTestResultReportQueryHandler(IFinalTestResultRepository finalTestResultRepository, IMapper mapper, IUserService userService, AuthContext authContext)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<TestResultRankingModel>> Handle(GetFinalTestResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestResultRankingModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            var finalTestResult = await _finalTestResultRepository.Queryable
                            .Include(x => x.SectionGroupResults)
                            .ThenInclude(x => x!.SectionGroup)
                            .Where(x => x.Id == request.FinalTestResultId && x.StudentId == student!.Id)
                            .FirstOrDefaultAsync(cancellationToken);

            var finalTestResultDto = _mapper.Map<TestResultRankingModel>(finalTestResult);
            if (finalTestResultDto != null)
            {
                finalTestResultDto.WorkingTime = DateTimeHelper.GetWorkingTime(finalTestResult?.CreatedDate, finalTestResult?.UpdatedDate ?? DateTime.UtcNow, finalTestResult.SectionGroupResults.Select(x => x.SectionGroup.ExecutionTime).FirstOrDefault());
            }

            methodResult.Result = finalTestResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
