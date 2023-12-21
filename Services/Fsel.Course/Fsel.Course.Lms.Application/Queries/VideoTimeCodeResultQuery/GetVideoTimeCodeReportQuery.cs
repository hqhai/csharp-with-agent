// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
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
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IMapper _mapper;

        public GetVideoTimeCodeReportQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository, IMapper mapper)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TestResultReportModel>> Handle(GetVideoTimeCodeReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TestResultReportModel> methodResult = new MethodResult<TestResultReportModel>();

            var videoTimeCodeResult = await _videoTimeCodeResultRepository.GetByIdAsync(request.VideoTimeCodeResultId);
            if (videoTimeCodeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult));
                return methodResult;
            }
            methodResult.Result = await GetVideoTimeCodeReport(videoTimeCodeResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<TestResultReportModel> GetVideoTimeCodeReport(VideoTimeCodeResult videoTimeCodeResult)
        {
            var query = _videoTimeCodeAnswerRepository.Queryable.Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id);
            var videoTimeCodeResultDto = _mapper.Map<TestResultReportModel>(videoTimeCodeResult);
            videoTimeCodeResultDto.CorrectQuestion = await query.Where(x => x.IsCorrect == true).CountAsync();
            videoTimeCodeResultDto.TotalQuestion = await query.CountAsync();
            return videoTimeCodeResultDto;
        }
    }
}
