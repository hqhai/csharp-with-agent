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
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoTimeCodeResultQuery : IRequest<MethodResult<VideoTimeCodeResultModel>>
    {
        public Guid VideoTimeCodeResultId { get; set; }

        public Guid VideoTimeCodeId { get; set; }
    }

    public class GetVideoTimeCodeResultQueryHandler : IRequestHandler<GetVideoTimeCodeResultQuery, MethodResult<VideoTimeCodeResultModel>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetVideoTimeCodeResultQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IMapper mapper, IUserService userService, AuthContext authContext)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _mapper = mapper;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<VideoTimeCodeResultModel>> Handle(GetVideoTimeCodeResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeResultModel> methodResult = new MethodResult<VideoTimeCodeResultModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable
                .Where(x => x.Id == request.VideoTimeCodeResultId && x.VideoTimeCodeId == request.VideoTimeCodeId && x.StudentId == student!.Id)
                .Select(x => new TestResultRankingModel
                {
                    Id = x.Id,
                    StudentId = x.StudentId,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    CreatedDate = x.CreatedDate,
                    CreatedFullName = x.CreatedFullName,
                    CreatedUserId = x.CreatedUserId,
                    Percent = x.Percent,
                    SkillScores = x.SkillScores,
                    Status = x.Status,
                    IsCurrentStudent = x.StudentId == student!.Id
                }).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = _mapper.Map<VideoTimeCodeResultModel>(videoTimeCodeResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
