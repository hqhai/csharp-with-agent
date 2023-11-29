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

    public class GetVideoTimeCodeReportQuery : IRequest<MethodResult<TestResultReportModel>>
    {
        public Guid VideoTimeCodeResultId { get; set; }
    }

    public class GetVideoTimeCodeReportQueryHandler : IRequestHandler<GetVideoTimeCodeReportQuery, MethodResult<TestResultReportModel>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IMapper _mapper;

        public GetVideoTimeCodeReportQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IMapper mapper)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TestResultReportModel>> Handle(GetVideoTimeCodeReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TestResultReportModel> methodResult = new MethodResult<TestResultReportModel>();

            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable
                .Where(x => x.Id == request.VideoTimeCodeResultId)
                .FirstOrDefaultAsync(cancellationToken);

            var videoTimeCodeResultDto = _mapper.Map<TestResultReportModel>(videoTimeCodeResult);
            if (videoTimeCodeResultDto != null)
            {
                videoTimeCodeResultDto.WorkingTime = DateTimeHelper.GetWorkingTime(videoTimeCodeResult?.CreatedDate, videoTimeCodeResult?.UpdatedDate ?? DateTime.UtcNow, videoTimeCodeResult!.VideoTimeCode!.ExecutionTime);
                videoTimeCodeResultDto.Score = videoTimeCodeResult.CorrectCount;
            }

            methodResult.Result = videoTimeCodeResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
