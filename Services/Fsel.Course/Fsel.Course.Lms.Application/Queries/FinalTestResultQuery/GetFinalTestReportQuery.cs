// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinalTestReportQuery : IRequest<MethodResult<FinalTestResultModel>>
    {
        public Guid FinalTestResultId { get; set; }
    }

    public class GetFinalTestReportQueryHandler : IRequestHandler<GetFinalTestReportQuery, MethodResult<FinalTestResultModel>>
    {
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public GetFinalTestReportQueryHandler(IFinalTestResultRepository finalTestResultRepository, IMapper mapper, AuthContext authContext, IUserService userService)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<FinalTestResultModel>> Handle(GetFinalTestReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestResultModel> methodResult = new MethodResult<FinalTestResultModel>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.FinalTestResultId);
            methodResult.Result = _mapper.Map<FinalTestResultModel>(finalTestResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
