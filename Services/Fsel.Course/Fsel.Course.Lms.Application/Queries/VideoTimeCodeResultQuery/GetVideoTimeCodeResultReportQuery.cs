// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery
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

    public class GetVideoTimeCodeResultReportQuery : IRequest<MethodResult<TestResultRankingModel>>
    {
        public Guid VideoTimeCodeResultId { get; set; }
    }

    public class GetVideoTimeCodeResultReportQueryHandler : IRequestHandler<GetVideoTimeCodeResultReportQuery, MethodResult<TestResultRankingModel>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetVideoTimeCodeResultReportQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IMapper mapper, IUserService userService, AuthContext authContext)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<TestResultRankingModel>> Handle(GetVideoTimeCodeResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TestResultRankingModel> methodResult = new MethodResult<TestResultRankingModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable
                .Where(x => x.Id == request.VideoTimeCodeResultId && x.StudentId == student!.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var videoTimeCodeResultDto = _mapper.Map<TestResultRankingModel>(videoTimeCodeResult);
            if (videoTimeCodeResultDto != null)
            {
                videoTimeCodeResultDto.WorkingTime = DateTimeHelper.GetWorkingTime(videoTimeCodeResult?.CreatedDate, videoTimeCodeResult?.UpdatedDate ?? DateTime.UtcNow, videoTimeCodeResult!.VideoTimeCode!.ExecutionTime);
            }

            methodResult.Result = videoTimeCodeResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
